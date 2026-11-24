using Microsoft.Extensions.Logging.Abstractions;
using SeoToolkit.Umbraco.Redirects.Core.Caching;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RegexRedirectsCacheTests
    {
        private static readonly Redirect[] FirstList = [new Redirect { OldUrl = "^/first" }];
        private static readonly Redirect[] SecondList = [new Redirect { OldUrl = "^/second" }];

        [Test]
        public void FirstReadLoadsSynchronously()
        {
            var cache = CreateCache(out _);

            var redirects = cache.Get(() => FirstList);

            Assert.That(redirects, Is.EqualTo(FirstList));
        }

        [Test]
        public void FreshListIsServedWithoutLoading()
        {
            var cache = CreateCache(out _);
            cache.Get(() => FirstList);
            var loads = 0;

            var redirects = cache.Get(() => { loads++; return SecondList; });

            Assert.That(redirects, Is.EqualTo(FirstList));
            Assert.That(loads, Is.Zero);
        }

        [Test]
        public async Task InvalidatedListIsServedWhileTheRefreshRuns()
        {
            var cache = CreateCache(out _);
            cache.Get(() => FirstList);
            cache.Invalidate();
            using var releaseLoad = new ManualResetEventSlim();

            var duringRefresh = cache.Get(() => { releaseLoad.Wait(); return SecondList; });
            var refresh = cache.StartRefresh(() => throw new InvalidOperationException("A second refresh must not start"));
            releaseLoad.Set();
            await refresh;

            Assert.That(duringRefresh, Is.EqualTo(FirstList));
            Assert.That(cache.Get(() => FirstList), Is.EqualTo(SecondList));
        }

        [Test]
        public async Task ExpiredListIsRefreshedInTheBackground()
        {
            var cache = CreateCache(out var timeProvider);
            cache.Get(() => FirstList);
            timeProvider.Advance(RegexRedirectsCache.MaxAge);

            var whileExpired = cache.Get(() => SecondList);
            await cache.StartRefresh(() => SecondList);

            Assert.That(whileExpired, Is.EqualTo(FirstList));
            Assert.That(cache.Get(() => FirstList), Is.EqualTo(SecondList));
        }

        [Test]
        public async Task FailedFirstLoadServesAnEmptyListAndRetries()
        {
            var cache = CreateCache(out _);

            var afterFailure = cache.Get(() => throw new TimeoutException());
            var retried = cache.Get(() => FirstList);
            await cache.StartRefresh(() => FirstList);

            Assert.That(afterFailure, Is.Empty);
            Assert.That(retried, Is.Empty);
            Assert.That(cache.Get(() => SecondList), Is.EqualTo(FirstList));
        }

        [Test]
        public async Task FailedRefreshKeepsThePreviousList()
        {
            var cache = CreateCache(out _);
            cache.Get(() => FirstList);
            cache.Invalidate();

            cache.Get(() => throw new TimeoutException());
            await cache.StartRefresh(() => throw new TimeoutException());

            Assert.That(cache.Get(() => FirstList), Is.EqualTo(FirstList));
        }

        private static RegexRedirectsCache CreateCache(out ManualTimeProvider timeProvider)
        {
            timeProvider = new ManualTimeProvider();
            return new RegexRedirectsCache(NullLogger<RegexRedirectsCache>.Instance, timeProvider);
        }

        private sealed class ManualTimeProvider : TimeProvider
        {
            private DateTimeOffset _utcNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public void Advance(TimeSpan duration) => _utcNow += duration;
        }
    }
}
