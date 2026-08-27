#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    public interface IHostPolitenessGate
    {
        /// <summary>Waits until it is acceptable to make another request to this host.</summary>
        Task WaitAsync(Uri url, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Keeps a minimum gap between requests to the same host.
    /// <para>
    /// Per host rather than globally, and that distinction matters twice over. Verifying external
    /// links must not consume the crawl's rate budget for the site being audited; and the site
    /// being audited is usually the customer's own live production site, so a crawl must never
    /// resemble an attack on it.
    /// </para>
    /// <para>
    /// A robots.txt Crawl-delay always wins if it is longer than the configured delay - the site
    /// is entitled to ask for more room, not less.
    /// </para>
    /// </summary>
    public sealed class HostPolitenessGate : IHostPolitenessGate, IDisposable
    {
        private sealed class HostState
        {
            public readonly SemaphoreSlim Gate = new(1, 1);
            public long NextAllowedTicks;
        }

        private readonly ConcurrentDictionary<string, HostState> _hosts = new(StringComparer.OrdinalIgnoreCase);
        private readonly TimeSpan _defaultDelay;
        private readonly Func<string, TimeSpan?>? _crawlDelayForHost;

        public HostPolitenessGate(TimeSpan defaultDelay, Func<string, TimeSpan?>? crawlDelayForHost = null)
        {
            _defaultDelay = defaultDelay < TimeSpan.Zero ? TimeSpan.Zero : defaultDelay;
            _crawlDelayForHost = crawlDelayForHost;
        }

        public async Task WaitAsync(Uri url, CancellationToken cancellationToken)
        {
            if (url is null) throw new ArgumentNullException(nameof(url));

            var delay = ResolveDelay(url.Host);
            if (delay <= TimeSpan.Zero) return;

            var state = _hosts.GetOrAdd(url.Authority, static _ => new HostState());

            // One request per host at a time while the gap is being observed, so several workers
            // landing on the same host queue up rather than all firing at once.
            await state.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var now = DateTime.UtcNow.Ticks;
                var nextAllowed = state.NextAllowedTicks;

                if (nextAllowed > now)
                    await Task.Delay(TimeSpan.FromTicks(nextAllowed - now), cancellationToken).ConfigureAwait(false);

                state.NextAllowedTicks = DateTime.UtcNow.Ticks + delay.Ticks;
            }
            finally
            {
                state.Gate.Release();
            }
        }

        private TimeSpan ResolveDelay(string host)
        {
            var fromRobots = _crawlDelayForHost?.Invoke(host);

            return fromRobots.HasValue && fromRobots.Value > _defaultDelay
                ? fromRobots.Value
                : _defaultDelay;
        }

        public void Dispose()
        {
            foreach (var state in _hosts.Values) state.Gate.Dispose();
            _hosts.Clear();
        }
    }
}
