using System.Linq;

namespace uSeoToolkit.Umbraco.Redirects.Core.Extensions
{
    internal static class StringExtensions
    {
        internal static string CleanUrl(this string url)
        {
            var urlParts = url.ToLowerInvariant().Split('?');
            var baseUrl = urlParts[0].TrimEnd('/');
            return urlParts.Length == 1 ? baseUrl : $"{baseUrl}?{string.Join("?", urlParts.Skip(1))}";
        }
    }
}
