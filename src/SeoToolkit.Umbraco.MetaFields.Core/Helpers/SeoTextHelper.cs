using System.Net;
using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Helpers
{
    public static partial class SeoTextHelper
    {
        private const string Ellipsis = "…";

        /// <summary>
        /// Converts stored rich text into the plain text a meta tag holds: tags stripped, entities decoded and whitespace collapsed.
        /// </summary>
        /// <remarks>
        /// Entities must be decoded because the field renderers HTML-encode the value; left encoded, <c>&amp;amp;</c> shows on the page as a literal <c>&amp;amp;</c>.
        /// </remarks>
        /// <param name="html">The stored HTML of a rich text value.</param>
        /// <returns>The plain text, or the input unchanged when it is null or empty.</returns>
        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            return CollapseWhitespace(WebUtility.HtmlDecode(html.StripHtml(" ")));
        }

        /// <summary>
        /// Replaces every run of whitespace, non-breaking spaces included, with a single space and trims the ends.
        /// </summary>
        /// <param name="text">The text to collapse.</param>
        /// <returns>The collapsed text, or the input unchanged when it is null or empty.</returns>
        public static string CollapseWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return WhitespaceRegex().Replace(text, " ").Trim();
        }

        /// <summary>
        /// Shortens text to at most <paramref name="maxLength"/> characters.
        /// </summary>
        /// <remarks>
        /// Cuts after the last sentence that ends in the second half of the allowance, so a description reads as complete;
        /// failing that, cuts at the last word boundary and appends an ellipsis.
        /// </remarks>
        /// <param name="text">The text to shorten.</param>
        /// <param name="maxLength">The maximum length of the result, ellipsis included.</param>
        /// <returns>The shortened text, or the input unchanged when it already fits.</returns>
        public static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || maxLength <= 0 || text.Length <= maxLength)
                return text;

            for (var i = maxLength - 1; i >= maxLength / 2; i--)
            {
                if ((text[i] is '.' or '!' or '?') && char.IsWhiteSpace(text[i + 1]))
                    return text[..(i + 1)];
            }

            var cut = text.LastIndexOf(' ', maxLength - Ellipsis.Length);
            if (cut <= 0)
                cut = maxLength - Ellipsis.Length;

            return text[..cut].TrimEnd(' ', ',', ';', ':', '-', '–', '—') + Ellipsis;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();
    }
}
