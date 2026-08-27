#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Everything about a check that is not its logic: how to show it, how to score it,
    /// what it needs from the crawler, and what it can be configured with.
    /// <para>
    /// This is the contract an add-on package writes against, so it is deliberately the
    /// widest part of the api. Getting it right up front matters more than anything else
    /// here - a missing field means a breaking change for everyone implementing checks.
    /// </para>
    /// </summary>
    public sealed record SeoCheckDescriptor
    {
        /// <summary>
        /// Stable identifier, persisted with every result and used in configuration.
        /// Prefix with your package name to avoid collisions, e.g. "Acme.Title.TooLong".
        /// Renaming an alias orphans historical results, so treat it as permanent.
        /// </summary>
        public required string Alias { get; init; }

        public required string Name { get; init; }

        public required SeoCheckCategory Category { get; init; }

        public string? Description { get; init; }

        /// <summary>Severity used when the check does not specify one per issue.</summary>
        public SeoSeverity DefaultSeverity { get; init; } = SeoSeverity.Warning;

        /// <summary>
        /// Relative importance within its category, 1 to 10. Drives the health score, so a
        /// missing title should outweigh a missing twitter card.
        /// </summary>
        public int Weight { get; init; } = 3;

        public SeoCheckCapabilities RequiredCapabilities { get; init; } = SeoCheckCapabilities.None;

        /// <summary>Resource kinds this check applies to. Anything else is skipped without calling the check.</summary>
        public ResourceKinds AppliesTo { get; init; } = ResourceKinds.HtmlPage;

        /// <summary>
        /// Whether this check makes sense when auditing a single page from the content app.
        /// Replaces the old AllowedAsPageCheck setting and the hard-coded exception for the
        /// broken link check, which only made sense for whole-site crawls.
        /// </summary>
        public bool SupportsSinglePage { get; init; } = true;

        /// <summary>Localisation key for remediation guidance shown next to a failure.</summary>
        public string? HowToFixKey { get; init; }

        public string? DocumentationUrl { get; init; }

        /// <summary>Options this check reads, keyed by option key.</summary>
        public IReadOnlyDictionary<string, SeoCheckOption> Options { get; init; }
            = new Dictionary<string, SeoCheckOption>(0);

        /// <summary>
        /// Fallback message templates per issue variant, used when no localisation entry exists.
        /// Placeholders are the keys of the issue's data, e.g. "Title is {Length} characters, maximum is {Max}".
        /// </summary>
        public IReadOnlyDictionary<string, string> MessageTemplates { get; init; }
            = new Dictionary<string, string>(0);

        /// <summary>Human readable name of the package supplying this check. Shown in the catalogue.</summary>
        public string? ProviderName { get; init; }

        /// <summary>
        /// Feature this check belongs to. When set and the feature is not enabled, the check is
        /// listed in the catalogue but never runs. See SeoFeatureRegistry.
        /// </summary>
        public string? RequiresFeature { get; init; }

        /// <summary>Convenience for building an option dictionary from a list.</summary>
        public static IReadOnlyDictionary<string, SeoCheckOption> OptionsFrom(params SeoCheckOption[] options)
        {
            if (options is null || options.Length == 0)
                return new Dictionary<string, SeoCheckOption>(0);

            var dictionary = new Dictionary<string, SeoCheckOption>(options.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var option in options)
                dictionary[option.Key] = option;
            return dictionary;
        }
    }
}
