using System;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    /// <summary>
    /// Resolves the placeholders a schema property value can contain against the content that is
    /// currently being rendered. A property either <em>is</em> a reference (the editor stores a
    /// reference key such as <c>[PageName]</c> or <c>[Property:title]</c>), or it is free text
    /// which may contain inline tokens such as <c>Hello {pageName}</c>.
    /// </summary>
    public static class SchemaReferenceResolver
    {
        public const string PageNameKey = "[PageName]";
        public const string PageUrlKey = "[PageUrl]";
        public const string SiteNameKey = "[SiteName]";
        public const string SiteUrlKey = "[SiteUrl]";

        public const string ContentPropertyPrefix = "[Property:";

        private const string ContentPropertyTokenPrefix = "property:";

        private static readonly Regex TokenRegex = new(@"\{([A-Za-z0-9_.:-]+)\}", RegexOptions.Compiled);

        public static string ResolveReference(string referenceKey, IPublishedContent currentContent)
        {
            if (currentContent is null || string.IsNullOrWhiteSpace(referenceKey))
                return string.Empty;

            if (TryGetContentPropertyAlias(referenceKey, out var alias))
                return GetContentPropertyValue(currentContent, alias) ?? string.Empty;

            return ResolveContextKey(referenceKey, currentContent) ?? string.Empty;
        }

        public static string ResolveTokens(string value, IPublishedContent currentContent)
        {
            if (currentContent is null || string.IsNullOrEmpty(value) || value.IndexOf('{') < 0)
                return value;

            return TokenRegex.Replace(value, match =>
            {
                var token = match.Groups[1].Value;

                if (token.StartsWith(ContentPropertyTokenPrefix, StringComparison.OrdinalIgnoreCase))
                    return GetContentPropertyValue(currentContent, token.Substring(ContentPropertyTokenPrefix.Length)) ?? string.Empty;

                var contextValue = ResolveContextKey($"[{token}]", currentContent);
                if (contextValue != null)
                    return contextValue;

                return GetContentPropertyValue(currentContent, token) ?? match.Value;
            });
        }
        
        public static string CreateContentPropertyKey(string alias)
            => $"{ContentPropertyPrefix}{alias}]";

        public static bool TryGetContentPropertyAlias(string referenceKey, out string alias)
        {
            alias = null;
            if (string.IsNullOrWhiteSpace(referenceKey)
                || referenceKey.StartsWith(ContentPropertyPrefix, StringComparison.OrdinalIgnoreCase) == false
                || referenceKey.EndsWith("]", StringComparison.Ordinal) == false)
                return false;

            alias = referenceKey.Substring(ContentPropertyPrefix.Length, referenceKey.Length - ContentPropertyPrefix.Length - 1);
            return string.IsNullOrWhiteSpace(alias) == false;
        }

        private static string ResolveContextKey(string referenceKey, IPublishedContent currentContent)
        {
            if (Matches(referenceKey, PageNameKey))
                return currentContent.Name ?? string.Empty;
            if (Matches(referenceKey, PageUrlKey))
                return currentContent.Url(mode: UrlMode.Absolute) ?? string.Empty;
            if (Matches(referenceKey, SiteNameKey))
                return currentContent.Root()?.Name ?? string.Empty;
            if (Matches(referenceKey, SiteUrlKey))
                return currentContent.Root()?.Url(mode: UrlMode.Absolute) ?? string.Empty;

            return null;
        }

        private static bool Matches(string referenceKey, string key)
            => string.Equals(referenceKey, key, StringComparison.OrdinalIgnoreCase);

        private static string GetContentPropertyValue(IPublishedContent currentContent, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return null;

            var property = currentContent.GetProperty(alias);
            if (property is null)
                return null;

            return property.GetValue()?.ToString() ?? string.Empty;
        }
    }
}
