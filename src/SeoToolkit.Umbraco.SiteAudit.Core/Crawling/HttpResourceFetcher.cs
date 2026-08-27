#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    public interface IResourceFetcher
    {
        /// <summary>
        /// Fetches one url. Never throws for a network problem - a failure is data a check may
        /// want to report, so it comes back on the resource rather than as an exception.
        /// </summary>
        Task<CrawledResource> FetchAsync(Uri url, SeoCheckCapabilities required, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Fetches resources over http, following redirects by hand so the chain is preserved.
    /// <para>
    /// Letting HttpClient follow redirects automatically hides exactly what redirect checks need
    /// to see: how many hops, whether they were permanent, and whether the chain loops. So
    /// redirects are followed here instead.
    /// </para>
    /// <para>
    /// Bodies are read with a hard size cap and only kept when something asked for them, so one
    /// enormous response cannot take the crawl down with it.
    /// </para>
    /// </summary>
    public sealed class HttpResourceFetcher : IResourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly CrawlOptions _options;

        public HttpResourceFetcher(HttpClient httpClient, CrawlOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<CrawledResource> FetchAsync(Uri url, SeoCheckCapabilities required, CancellationToken cancellationToken)
        {
            if (url is null) throw new ArgumentNullException(nameof(url));

            var startedUtc = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            var redirects = new List<RedirectHop>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { url.AbsoluteUri };

            var current = url;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_options.RequestTimeout);

            try
            {
                for (var hop = 0; ; hop++)
                {
                    using var request = CreateRequest(current);

                    // ResponseHeadersRead so the body is streamed and the size cap can be
                    // enforced before it is all in memory.
                    using var response = await _httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token)
                        .ConfigureAwait(false);

                    var timeToFirstByte = (int)stopwatch.ElapsedMilliseconds;
                    var statusCode = (int)response.StatusCode;

                    if (IsRedirect(statusCode) && response.Headers.Location is not null)
                    {
                        var next = response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location
                            : new Uri(current, response.Headers.Location);

                        redirects.Add(new RedirectHop(current, next, statusCode));

                        if (!visited.Add(next.AbsoluteUri))
                        {
                            return Failed(url, next, CrawlFailureReason.RedirectLoop,
                                "The redirect chain returns to a url it already visited.",
                                startedUtc, stopwatch, redirects, isLoop: true);
                        }

                        if (hop >= _options.MaxRedirects)
                        {
                            return Failed(url, next, CrawlFailureReason.TooManyRedirects,
                                $"More than {_options.MaxRedirects} redirects.",
                                startedUtc, stopwatch, redirects);
                        }

                        current = next;
                        continue;
                    }

                    return await BuildResourceAsync(url, current, response, required, startedUtc,
                        stopwatch, timeToFirstByte, redirects, timeout.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Failed(url, current, CrawlFailureReason.Cancelled, "The audit was stopped.",
                    startedUtc, stopwatch, redirects);
            }
            catch (OperationCanceledException)
            {
                return Failed(url, current, CrawlFailureReason.Timeout,
                    $"No response within {_options.RequestTimeout.TotalSeconds:0} seconds.",
                    startedUtc, stopwatch, redirects);
            }
            catch (HttpRequestException ex)
            {
                return Failed(url, current, ClassifyHttpFailure(ex), ex.Message,
                    startedUtc, stopwatch, redirects);
            }
            catch (Exception ex)
            {
                return Failed(url, current, CrawlFailureReason.Unknown, ex.Message,
                    startedUtc, stopwatch, redirects);
            }
        }

        private HttpRequestMessage CreateRequest(Uri url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.TryParseAdd(_options.UserAgent);
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,*/*;q=0.8");

            if (_options.Authentication.TryGetValue(url.Host, out var auth))
                ApplyAuthentication(request, auth);

            return request;
        }

        private static void ApplyAuthentication(HttpRequestMessage request, CrawlAuthentication auth)
        {
            switch (auth.Type)
            {
                case CrawlAuthenticationType.Basic:
                    var raw = Encoding.UTF8.GetBytes($"{auth.UserName}:{auth.Password}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(raw));
                    break;

                case CrawlAuthenticationType.Bearer:
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
                    break;

                case CrawlAuthenticationType.Header:
                    if (!string.IsNullOrEmpty(auth.HeaderName))
                        request.Headers.TryAddWithoutValidation(auth.HeaderName!, auth.Token);
                    break;
            }
        }

        private async Task<CrawledResource> BuildResourceAsync(Uri requestedUrl, Uri finalUrl,
            HttpResponseMessage response, SeoCheckCapabilities required, DateTimeOffset startedUtc,
            Stopwatch stopwatch, int timeToFirstByte, List<RedirectHop> redirects, CancellationToken cancellationToken)
        {
            var contentType = response.Content.Headers.ContentType?.MediaType;
            var kind = ClassifyKind(contentType);

            // Markup is always needed to find links and build facts. Anything else is only read
            // if a check asked for raw bodies.
            var wantsBody = kind == ResourceKind.HtmlPage || required.Requires(SeoCheckCapabilities.RawBody);

            string? body = null;
            long transferred = 0;
            CrawlFailureReason? failure = null;
            string? failureDetail = null;

            if (wantsBody && response.Content is not null)
            {
                var (content, bytes, tooLarge) = await ReadBodyAsync(response, cancellationToken).ConfigureAwait(false);
                transferred = bytes;

                if (tooLarge)
                {
                    failure = CrawlFailureReason.ResponseTooLarge;
                    failureDetail = $"The response exceeded the {_options.MaxResponseBytes} byte limit.";
                }
                else
                {
                    body = content;
                }
            }

            stopwatch.Stop();

            return new CrawledResource
            {
                RequestedUrl = requestedUrl,
                FinalUrl = finalUrl,
                NormalizedUrl = requestedUrl.AbsoluteUri,
                UrlHash = string.Empty,
                StatusCode = (int)response.StatusCode,
                ResponseHeaders = required.Requires(SeoCheckCapabilities.ResponseHeaders)
                    ? CollectHeaders(response)
                    : new Dictionary<string, string[]>(0),
                ContentType = contentType,
                CharSet = response.Content?.Headers.ContentType?.CharSet,
                DeclaredContentLength = response.Content?.Headers.ContentLength,
                TransferredBytes = transferred,
                ContentEncoding = FirstOrNull(response.Content?.Headers.ContentEncoding),
                ETag = response.Headers.ETag?.Tag,
                LastModified = response.Content?.Headers.LastModified,
                Protocol = response.Version.ToString(),
                RedirectChain = redirects,
                RawBody = body,
                Kind = kind,
                RequestStartedUtc = startedUtc,
                RequestCompletedUtc = DateTimeOffset.UtcNow,
                TimeToFirstByteMs = timeToFirstByte,
                TotalMs = (int)stopwatch.ElapsedMilliseconds,
                Failure = failure,
                FailureDetail = failureDetail
            };
        }

        /// <summary>
        /// Reads the body, giving up once the cap is exceeded rather than buffering the rest.
        /// </summary>
        private async Task<(string? Content, long Bytes, bool TooLarge)> ReadBodyAsync(
            HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // Trust a declared length that is already over the limit and skip the read entirely.
            if (response.Content.Headers.ContentLength > _options.MaxResponseBytes)
                return (null, response.Content.Headers.ContentLength.Value, true);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            var buffer = ArrayPool<byte>.Shared.Rent(81920);
            using var memory = new MemoryStream();
            try
            {
                while (true)
                {
                    var read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                    if (read == 0) break;

                    if (memory.Length + read > _options.MaxResponseBytes)
                        return (null, memory.Length + read, true);

                    memory.Write(buffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            var encoding = ResolveEncoding(response.Content.Headers.ContentType?.CharSet);
            return (encoding.GetString(memory.GetBuffer(), 0, (int)memory.Length), memory.Length, false);
        }

        private static Encoding ResolveEncoding(string? charSet)
        {
            if (string.IsNullOrWhiteSpace(charSet)) return Encoding.UTF8;

            try
            {
                return Encoding.GetEncoding(charSet!.Trim('"'));
            }
            catch (ArgumentException)
            {
                // A server declaring a charset nobody recognises is not a reason to lose the page.
                return Encoding.UTF8;
            }
        }

        private static IReadOnlyDictionary<string, string[]> CollectHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var header in response.Headers)
                headers[header.Key.ToLowerInvariant()] = header.Value as string[] ?? System.Linq.Enumerable.ToArray(header.Value);

            if (response.Content is not null)
            {
                foreach (var header in response.Content.Headers)
                    headers[header.Key.ToLowerInvariant()] = header.Value as string[] ?? System.Linq.Enumerable.ToArray(header.Value);
            }

            return headers;
        }

        private CrawledResource Failed(Uri requestedUrl, Uri finalUrl, CrawlFailureReason reason,
            string detail, DateTimeOffset startedUtc, Stopwatch stopwatch,
            List<RedirectHop> redirects, bool isLoop = false)
        {
            stopwatch.Stop();

            return new CrawledResource
            {
                RequestedUrl = requestedUrl,
                FinalUrl = finalUrl,
                NormalizedUrl = requestedUrl.AbsoluteUri,
                UrlHash = string.Empty,
                StatusCode = 0,
                Kind = ResourceKind.Unknown,
                RedirectChain = redirects,
                IsRedirectLoop = isLoop,
                RequestStartedUtc = startedUtc,
                RequestCompletedUtc = DateTimeOffset.UtcNow,
                TotalMs = (int)stopwatch.ElapsedMilliseconds,
                Failure = reason,
                FailureDetail = detail
            };
        }

        private static bool IsRedirect(int statusCode)
            => statusCode is 301 or 302 or 303 or 307 or 308;

        private static CrawlFailureReason ClassifyHttpFailure(HttpRequestException exception)
        {
            var inner = exception.InnerException;

            if (inner is System.Security.Authentication.AuthenticationException)
                return CrawlFailureReason.CertificateFailure;

            if (inner is System.Net.Sockets.SocketException socket)
            {
                return socket.SocketErrorCode is System.Net.Sockets.SocketError.HostNotFound
                    or System.Net.Sockets.SocketError.NoData
                    ? CrawlFailureReason.DnsFailure
                    : CrawlFailureReason.ConnectionFailure;
            }

            return CrawlFailureReason.ConnectionFailure;
        }

        private static string? FirstOrNull(IEnumerable<string>? values)
        {
            if (values is null) return null;
            foreach (var value in values) return value;
            return null;
        }

        /// <summary>Works out what a resource is from its content type.</summary>
        public static ResourceKind ClassifyKind(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return ResourceKind.Unknown;

            var type = contentType!.ToLowerInvariant();

            if (type.Contains("html") || type.Contains("xhtml")) return ResourceKind.HtmlPage;
            if (type.StartsWith("image/")) return ResourceKind.Image;
            if (type.StartsWith("video/") || type.StartsWith("audio/")) return ResourceKind.Media;
            if (type.Contains("css")) return ResourceKind.Stylesheet;
            if (type.Contains("javascript") || type.Contains("ecmascript")) return ResourceKind.Script;

            if (type.Contains("pdf") || type.Contains("msword") || type.Contains("officedocument") ||
                type.Contains("ms-excel") || type.Contains("ms-powerpoint") || type.Contains("opendocument"))
                return ResourceKind.Document;

            return ResourceKind.Other;
        }

        /// <summary>
        /// Builds the handler the crawl client should use. Redirects are never followed
        /// automatically, because the chain is something checks need to see.
        /// </summary>
        public static HttpMessageHandler CreateHandler(CrawlOptions options)
            => new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.All,
                ConnectTimeout = TimeSpan.FromSeconds(15),
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = options.AllowInvalidCertificates
                        ? (_, _, _, _) => true
                        : null
                }
            };
    }
}
