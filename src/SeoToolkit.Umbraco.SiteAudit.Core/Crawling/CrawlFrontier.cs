#nullable enable
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>One url waiting to be crawled.</summary>
    public sealed class CrawlRequest
    {
        public required Uri Url { get; init; }
        public required string NormalizedUrl { get; init; }
        public int Depth { get; init; }
        public Uri? Referrer { get; init; }
        public DiscoverySource DiscoveredVia { get; init; }
    }

    /// <summary>
    /// The queue of urls still to visit, plus the record of which ones have already been seen.
    /// <para>
    /// Replaces the old scheduler, which was a plain queue and a list with an O(n) membership
    /// test, was not safe to touch from more than one thread, and compared raw
    /// <see cref="Uri"/> values so the same page reached by two slightly different urls was
    /// crawled twice.
    /// </para>
    /// </summary>
    public sealed class CrawlFrontier
    {
        private readonly Channel<CrawlRequest> _channel;
        private readonly ConcurrentDictionary<string, byte> _seen = new(StringComparer.Ordinal);
        private readonly TaskCompletionSource _drained =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly int? _maxPages;
        private readonly int? _maxDepth;

        private int _admitted;
        private int _outstanding;
        private int _completed;

        public CrawlFrontier(int? maxPages = null, int? maxDepth = null)
        {
            _maxPages = maxPages;
            _maxDepth = maxDepth;
            _channel = Channel.CreateUnbounded<CrawlRequest>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
        }

        /// <summary>How many distinct urls have been admitted to the queue.</summary>
        public int Discovered => Volatile.Read(ref _admitted);

        /// <summary>How many have been fully processed.</summary>
        public int Completed => Volatile.Read(ref _completed);

        /// <summary>Completes once every admitted url has been processed.</summary>
        public Task Drained => _drained.Task;

        public ChannelReader<CrawlRequest> Reader => _channel.Reader;

        /// <summary>
        /// Admits a url unless it has been seen before or a limit says otherwise.
        /// <para>
        /// Callers must enqueue everything a page discovered <em>before</em> calling
        /// <see cref="Complete"/> for that page. Doing it the other way round lets the
        /// outstanding count reach zero while there is still work to schedule, and the crawl
        /// finishes early.
        /// </para>
        /// </summary>
        public bool TryEnqueue(CrawlRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            if (_maxDepth.HasValue && request.Depth > _maxDepth.Value) return false;

            // Claim the url first: two workers finding the same link at the same moment must not
            // both queue it.
            if (!_seen.TryAdd(request.NormalizedUrl, 0)) return false;

            var admitted = Interlocked.Increment(ref _admitted);
            if (_maxPages.HasValue && admitted > _maxPages.Value)
            {
                // Leave it marked as seen: the limit is a reason never to fetch it, not a
                // reason to keep rediscovering it.
                Interlocked.Decrement(ref _admitted);
                return false;
            }

            Interlocked.Increment(ref _outstanding);

            if (_channel.Writer.TryWrite(request)) return true;

            // The channel is closed, so the crawl is already finishing.
            Interlocked.Decrement(ref _outstanding);
            return false;
        }

        /// <summary>Marks one url as fully processed, and closes the queue once none are left.</summary>
        public void Complete(CrawlRequest request)
        {
            Interlocked.Increment(ref _completed);

            if (Interlocked.Decrement(ref _outstanding) != 0) return;

            // Nothing queued and nothing in flight: no worker can produce more work, because a
            // worker only enqueues while it still holds an outstanding item of its own.
            _channel.Writer.TryComplete();
            _drained.TrySetResult();
        }

        /// <summary>Abandons the crawl, releasing anything waiting on the queue.</summary>
        public void Abort()
        {
            _channel.Writer.TryComplete();
            _drained.TrySetResult();
        }

        public bool HasSeen(string normalizedUrl) => _seen.ContainsKey(normalizedUrl);

        /// <summary>
        /// Records a url as seen without queueing it, for urls that are known but deliberately
        /// not crawled - external links, or paths robots.txt blocks.
        /// </summary>
        public bool MarkSeen(string normalizedUrl) => _seen.TryAdd(normalizedUrl, 0);

        /// <summary>Every url the crawl has encountered, including ones it chose not to fetch.</summary>
        public int SeenCount => _seen.Count;
    }
}
