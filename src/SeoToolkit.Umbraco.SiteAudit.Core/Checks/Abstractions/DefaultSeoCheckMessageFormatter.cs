#nullable enable
using System;
using System.Text;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Resolves a message from the check's own templates, substituting the issue's data.
    /// <para>
    /// Lookup order is localisation key, then the template the check declared for that variant,
    /// then its default template, then the check name. Something readable always comes back,
    /// so a check that forgets a template degrades rather than breaks.
    /// </para>
    /// </summary>
    public class DefaultSeoCheckMessageFormatter : ISeoCheckMessageFormatter
    {
        /// <summary>Template key used when an issue has no variant.</summary>
        public const string DefaultVariantKey = "";

        public string Format(SeoCheckIssue issue, SeoCheckDescriptor descriptor)
        {
            if (issue is null) throw new ArgumentNullException(nameof(issue));
            if (descriptor is null) throw new ArgumentNullException(nameof(descriptor));

            var template = ResolveTemplate(issue, descriptor);
            if (string.IsNullOrEmpty(template))
                return descriptor.Name;

            return Substitute(template!, issue);
        }

        /// <summary>
        /// Hook for a localisation-aware implementation. Returning null falls through to the
        /// templates the check declared.
        /// </summary>
        protected virtual string? ResolveLocalised(SeoCheckIssue issue, SeoCheckDescriptor descriptor) => null;

        private string? ResolveTemplate(SeoCheckIssue issue, SeoCheckDescriptor descriptor)
        {
            var localised = ResolveLocalised(issue, descriptor);
            if (!string.IsNullOrEmpty(localised)) return localised;

            var templates = descriptor.MessageTemplates;
            if (templates.Count == 0) return null;

            if (!string.IsNullOrEmpty(issue.Variant) &&
                templates.TryGetValue(issue.Variant!, out var variantTemplate))
                return variantTemplate;

            return templates.TryGetValue(DefaultVariantKey, out var fallback) ? fallback : null;
        }

        /// <summary>
        /// Replaces {Placeholder} tokens with values from the issue's data. An unknown token is
        /// left as it stands rather than blanked, so a typo in a template is visible instead of
        /// producing a sentence with a hole in it.
        /// </summary>
        private static string Substitute(string template, SeoCheckIssue issue)
        {
            if (template.IndexOf('{') < 0) return template;

            var result = new StringBuilder(template.Length + 32);
            var index = 0;

            while (index < template.Length)
            {
                var open = template.IndexOf('{', index);
                if (open < 0)
                {
                    result.Append(template, index, template.Length - index);
                    break;
                }

                var close = template.IndexOf('}', open + 1);
                if (close < 0)
                {
                    result.Append(template, index, template.Length - index);
                    break;
                }

                result.Append(template, index, open - index);

                var key = template.Substring(open + 1, close - open - 1);
                if (issue.Data.TryGetValue(key, out var value))
                    result.Append(value);
                else
                    result.Append(template, open, close - open + 1);

                index = close + 1;
            }

            return result.ToString();
        }
    }
}
