#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    public interface IUrlNormalizer
    {
        /// <summary>Canonical string form of a url, or null when it is not something worth crawling.</summary>
        string? Normalize(Uri url);

        /// <summary>Stable hash of a normalised url, used as the persistence key.</summary>
        string Hash(string normalizedUrl);
    }

    /// <summary>
    /// Reduces urls to a canonical form so the same page is not crawled twice.
    /// <para>
    /// The old crawler compared <see cref="Uri"/> objects directly, which meant a page reached
    /// with a tracking parameter, a different capitalisation of the host, or a trailing slash was
    /// treated as a separate page - inflating page counts and re-running every check against the
    /// same content.
    /// </para>
    /// </summary>
    public sealed class DefaultUrlNormalizer : IUrlNormalizer
    {
        private readonly CrawlOptions _options;
        private readonly string[] _exactStrips;
        private readonly string[] _prefixStrips;

        public DefaultUrlNormalizer(CrawlOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // Split the strip list once rather than re-examining every entry for a trailing
            // wildcard on every url of the crawl.
            var patterns = options.StripQueryParameters ?? Array.Empty<string>();
            _exactStrips = patterns.Where(it => !it.EndsWith('*')).ToArray();
            _prefixStrips = patterns.Where(it => it.EndsWith('*'))
                .Select(it => it[..^1])
                .ToArray();
        }

        public string? Normalize(Uri url)
        {
            if (url is null) return null;
            if (!url.IsAbsoluteUri) return null;

            // Only http(s) is crawlable. mailto, tel, javascript and friends are links, but not pages.
            if (!string.Equals(url.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                return null;

            var scheme = url.Scheme.ToLowerInvariant();

            // IdnHost gives the punycode form, so a unicode domain and its encoded twin agree.
            var host = url.IdnHost.ToLowerInvariant();

            var builder = new StringBuilder(url.OriginalString.Length + 8);
            builder.Append(scheme).Append("://").Append(host);

            if (!url.IsDefaultPort)
                builder.Append(':').Append(url.Port);

            builder.Append(NormalizePath(url.AbsolutePath));

            var query = NormalizeQuery(url.Query);
            if (!string.IsNullOrEmpty(query))
                builder.Append('?').Append(query);

            // The fragment is never sent to the server, so /page and /page#section are one page.
            return builder.ToString();
        }

        public string Hash(string normalizedUrl)
        {
            if (normalizedUrl is null) throw new ArgumentNullException(nameof(normalizedUrl));

            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(normalizedUrl));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "/";

            switch (_options.TrailingSlash)
            {
                case TrailingSlashPolicy.Remove:
                    // Never strip the root itself - "https://example.com" without a path is wrong.
                    return path.Length > 1 && path.EndsWith('/') ? path.TrimEnd('/') : path;

                case TrailingSlashPolicy.Add:
                    if (path.EndsWith('/')) return path;
                    // A path with an extension is a file, not a directory.
                    var lastSegment = path[(path.LastIndexOf('/') + 1)..];
                    return lastSegment.Contains('.') ? path : path + "/";

                default:
                    return path;
            }
        }

        private string NormalizeQuery(string query)
        {
            if (string.IsNullOrEmpty(query) || query == "?") return string.Empty;

            var pairs = query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Where(it => !IsStripped(it))
                .ToList();

            if (pairs.Count == 0) return string.Empty;

            if (_options.SortQueryParameters)
                pairs.Sort(StringComparer.Ordinal);

            return string.Join('&', pairs);
        }

        private bool IsStripped(string pair)
        {
            var separator = pair.IndexOf('=');
            var key = separator < 0 ? pair : pair[..separator];

            foreach (var strip in _exactStrips)
            {
                if (string.Equals(key, strip, StringComparison.OrdinalIgnoreCase)) return true;
            }

            foreach (var prefix in _prefixStrips)
            {
                if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
    }
}
