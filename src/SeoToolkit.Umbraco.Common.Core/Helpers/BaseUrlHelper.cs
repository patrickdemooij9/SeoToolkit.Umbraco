using System;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public static class BaseUrlHelper
    {
        /// <summary>
        /// Replaces the scheme and host of a URL with the configured BaseUrl.
        /// If baseUrl is null or empty, the original URL is returned unchanged.
        /// </summary>
        public static string ApplyBaseUrl(string url, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) return url;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var originalUri)) return url;

            var normalizedBaseUrl = baseUrl.TrimEnd('/');
            if (!normalizedBaseUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                normalizedBaseUrl = $"https://{normalizedBaseUrl}";
            }

            if (!Uri.TryCreate(normalizedBaseUrl, UriKind.Absolute, out var baseUri)) return url;

            var builder = new UriBuilder(originalUri)
            {
                Scheme = baseUri.Scheme,
                Host = baseUri.Host,
                Port = baseUri.Port
            };
            return builder.Uri.AbsoluteUri;
        }
    }
}
