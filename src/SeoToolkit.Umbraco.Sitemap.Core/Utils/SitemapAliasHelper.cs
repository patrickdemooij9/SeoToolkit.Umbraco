using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SeoToolkit.Umbraco.Sitemap.Core.Utils
{
    public static class SitemapAliasHelper
    {
        private static readonly string[] TypeNameSuffixes = { "SitemapCollectionProvider", "SitemapCollection", "SitemapProvider", "CollectionProvider", "Provider" };
        private static readonly Regex ValidAlias = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

        public static bool IsValid(string alias) => !string.IsNullOrEmpty(alias) && ValidAlias.IsMatch(alias);

        //"NewsArticlesSitemapCollectionProvider" -> "news-articles"
        public static string FromType(Type type)
        {
            var name = type.Name;
            foreach (var suffix in TypeNameSuffixes)
            {
                if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    name = name[..^suffix.Length];
                    break;
                }
            }

            var builder = new StringBuilder();
            foreach (var character in name)
            {
                if (!char.IsLetterOrDigit(character))
                {
                    if (builder.Length > 0 && builder[^1] != '-') builder.Append('-');
                    continue;
                }

                if (char.IsUpper(character) && builder.Length > 0 && builder[^1] != '-')
                    builder.Append('-');
                builder.Append(char.ToLowerInvariant(character));
            }
            return builder.ToString().Trim('-');
        }
    }
}
