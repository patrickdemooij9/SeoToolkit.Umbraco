using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    /// <summary>
    /// Holds the enabled regex redirects in memory. Every request reads them, so after the first load a
    /// stale or invalidated list is refreshed in the background and the current one is served meanwhile:
    /// a slow database never holds up the requests waiting on it.
    /// </summary>
    public sealed class RegexRedirectsCache
    {
        /// <summary>How long a loaded list is served before a background refresh is started.</summary>
        public static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);

        private readonly ILogger<RegexRedirectsCache> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly object _initialLoadLock = new();
        private readonly object _refreshLock = new();

        private Snapshot _snapshot;
        private Task _refreshTask = Task.CompletedTask;
        private int _version;

        public RegexRedirectsCache(ILogger<RegexRedirectsCache> logger, TimeProvider timeProvider)
        {
            _logger = logger;
            _timeProvider = timeProvider;
        }

        /// <summary>
        /// Returns the cached regex redirects. Only the first call loads synchronously; later calls return
        /// the current list and refresh it in the background when it is stale or invalidated.
        /// </summary>
        /// <param name="load">Reads the enabled regex redirects from the database.</param>
        public IReadOnlyCollection<Redirect> Get(Func<Redirect[]> load)
        {
            var snapshot = Volatile.Read(ref _snapshot);
            if (snapshot is null)
            {
                return LoadInitial(load);
            }

            if (IsStale(snapshot))
            {
                StartRefresh(load);
            }

            return snapshot.Redirects;
        }

        /// <summary>Marks the cached list stale so the next read refreshes it.</summary>
        public void Invalidate() => Interlocked.Increment(ref _version);

        /// <summary>Starts a background refresh unless one is already running.</summary>
        /// <param name="load">Reads the enabled regex redirects from the database.</param>
        /// <returns>The running refresh.</returns>
        public Task StartRefresh(Func<Redirect[]> load)
        {
            lock (_refreshLock)
            {
                if (_refreshTask.IsCompleted)
                {
                    _refreshTask = Task.Run(() => Refresh(load));
                }

                return _refreshTask;
            }
        }

        private IReadOnlyCollection<Redirect> LoadInitial(Func<Redirect[]> load)
        {
            lock (_initialLoadLock)
            {
                if (Volatile.Read(ref _snapshot) is null)
                {
                    Refresh(load);
                }
            }

            return Volatile.Read(ref _snapshot).Redirects;
        }

        private void Refresh(Func<Redirect[]> load)
        {
            var version = Volatile.Read(ref _version);
            try
            {
                Volatile.Write(ref _snapshot, new Snapshot(load(), _timeProvider.GetUtcNow(), version));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not load the regex redirects; serving the previous list until the next refresh");

                // A failed first load leaves an empty, already-stale list, so callers stop waiting on the
                // database and the next read retries in the background.
                Interlocked.CompareExchange(
                    ref _snapshot,
                    new Snapshot([], _timeProvider.GetUtcNow() - MaxAge, version),
                    null);
            }
        }

        private bool IsStale(Snapshot snapshot) =>
            snapshot.Version != Volatile.Read(ref _version) ||
            _timeProvider.GetUtcNow() - snapshot.LoadedAt >= MaxAge;

        private sealed record Snapshot(Redirect[] Redirects, DateTimeOffset LoadedAt, int Version);
    }
}
