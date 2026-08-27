#nullable enable
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>
    /// Shared by the duplicate checks. Each supplies a facet and its wording; the grouping work
    /// is done once by the crawl index and reused, rather than each check walking the corpus.
    /// </summary>
    public abstract class DuplicateCheckBase : SeoSiteCheckBase
    {
        protected abstract DuplicateFacet Facet { get; }

        /// <summary>Whether the duplicated value itself is worth putting in the message.</summary>
        protected virtual bool IncludeValue => true;

        protected override void Evaluate(SeoSiteCheckContext context)
        {
            foreach (var group in context.Index.DuplicatesBy(Facet))
            {
                foreach (var page in group)
                {
                    var issue = context.Report(page.Url).Data("Count", group.Count);

                    if (IncludeValue)
                    {
                        var value = Value(page);
                        if (value is not null) issue.Data("Value", value).Evidence(value);
                    }

                    // The other pages sharing this value, so the finding can be acted on without
                    // going looking for them.
                    foreach (var other in group)
                        if (!ReferenceEquals(other, page)) issue.Related(other.Url);
                }
            }
        }

        protected virtual string? Value(ResourceSummary summary) => null;
    }

    public sealed class DuplicateTitleCheck : DuplicateCheckBase
    {
        protected override DuplicateFacet Facet => DuplicateFacet.Title;

        protected override string? Value(ResourceSummary summary) => summary.Title;

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DuplicateTitle",
            Name = "Duplicate title",
            Description = "Pages sharing a title compete with each other in search results.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Count} pages share the title \"{Value}\"."
            }
        };
    }

    public sealed class DuplicateDescriptionCheck : DuplicateCheckBase
    {
        protected override DuplicateFacet Facet => DuplicateFacet.MetaDescription;

        protected override string? Value(ResourceSummary summary) => summary.MetaDescription;

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DuplicateDescription",
            Name = "Duplicate meta description",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 4,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Count} pages share this meta description."
            }
        };
    }

    public sealed class DuplicateH1Check : DuplicateCheckBase
    {
        protected override DuplicateFacet Facet => DuplicateFacet.H1;

        protected override string? Value(ResourceSummary summary) => summary.H1;

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DuplicateH1",
            Name = "Duplicate H1",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Count} pages share the heading \"{Value}\"."
            }
        };
    }

    /// <summary>
    /// Pages whose body text is identical. Matched on the stored hash, because the text itself is
    /// never retained - keeping a whole site's page text in memory is not something the crawler
    /// can afford.
    /// </summary>
    public sealed class DuplicateContentCheck : DuplicateCheckBase
    {
        protected override DuplicateFacet Facet => DuplicateFacet.Content;

        protected override bool IncludeValue => false;

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "ExactDuplicateContent",
            Name = "Duplicate content",
            Description = "Pages with identical text give search engines no reason to prefer either.",
            Category = SeoCheckCategory.Content,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 8,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Count} pages have exactly the same text."
            }
        };
    }
}
