#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>A page with no title has nothing to show in a result listing.</summary>
    public sealed class TitleMissingCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "TitleMissing",
            Name = "Missing title",
            Description = "Every page needs a title element.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 10,
            MessageTemplates = new Dictionary<string, string> { [""] = "This page has no title." }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.Title)) context.Report();
        }
    }

    /// <summary>Titles beyond roughly this length are cut off in results.</summary>
    public sealed class TitleLengthCheck : HtmlPageCheckBase
    {
        public const string MaxOption = "MaxLength";
        public const string MinOption = "MinLength";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "TitleLength",
            Name = "Title length",
            Description = "Titles should be long enough to be descriptive and short enough not to be truncated.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum length", 60, 20, 200),
                SeoCheckOption.Int(MinOption, "Minimum length", 15, 1, 200)),
            MessageTemplates = new Dictionary<string, string>
            {
                ["TooLong"] = "The title is {Length} characters; anything over {Max} is likely to be truncated.",
                ["TooShort"] = "The title is only {Length} characters; aim for at least {Min}."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            // A missing title is TitleMissingCheck's finding, not this one's - reporting it twice
            // would count the same problem against the score in two places.
            if (string.IsNullOrWhiteSpace(facts.Title)) return;

            var max = context.Options.Get(MaxOption, 60);
            var min = context.Options.Get(MinOption, 15);
            var length = facts.Title!.Length;

            if (length > max)
                context.Report("TooLong").Data("Length", length).Data("Max", max).Evidence(facts.Title);
            else if (length < min)
                context.Report("TooShort").Data("Length", length).Data("Min", min).Evidence(facts.Title);
        }
    }

    /// <summary>More than one title element leaves it to the search engine to pick.</summary>
    public sealed class MultipleTitlesCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MultipleTitles",
            Name = "Multiple titles",
            Description = "A page should have exactly one title element.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 5,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page has {Count} title elements."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.TitleCount > 1) context.Report().Data("Count", facts.TitleCount);
        }
    }

    public sealed class DescriptionMissingCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DescriptionMissing",
            Name = "Missing meta description",
            Description = "Without one, search engines invent a summary from the page text.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string> { [""] = "This page has no meta description." }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.MetaDescription)) context.Report();
        }
    }

    public sealed class DescriptionLengthCheck : HtmlPageCheckBase
    {
        public const string MaxOption = "MaxLength";
        public const string MinOption = "MinLength";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DescriptionLength",
            Name = "Meta description length",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 4,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum length", 160, 50, 400),
                SeoCheckOption.Int(MinOption, "Minimum length", 70, 1, 400)),
            MessageTemplates = new Dictionary<string, string>
            {
                ["TooLong"] = "The description is {Length} characters; anything over {Max} is likely to be truncated.",
                ["TooShort"] = "The description is only {Length} characters; aim for at least {Min}."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.MetaDescription)) return;

            var max = context.Options.Get(MaxOption, 160);
            var min = context.Options.Get(MinOption, 70);
            var length = facts.MetaDescription!.Length;

            if (length > max)
                context.Report("TooLong").Data("Length", length).Data("Max", max).Evidence(facts.MetaDescription);
            else if (length < min)
                context.Report("TooShort").Data("Length", length).Data("Min", min).Evidence(facts.MetaDescription);
        }
    }

    public sealed class H1MissingCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "H1Missing",
            Name = "Missing H1",
            Description = "The main heading tells readers and search engines what the page is about.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string> { [""] = "This page has no H1 heading." }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.H1s.Count == 0) context.Report();
        }
    }

    public sealed class MultipleH1Check : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MultipleH1",
            Name = "Multiple H1 headings",
            Description = "More than one main heading makes the page's subject ambiguous.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string> { [""] = "This page has {Count} H1 headings." }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.H1s.Count > 1) context.Report().Data("Count", facts.H1s.Count).Evidence(facts.H1s[0]);
        }
    }

    /// <summary>
    /// Headings that jump a level - an h4 straight after an h2 - break the document outline that
    /// screen readers and search engines rely on.
    /// </summary>
    public sealed class HeadingOrderCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "HeadingLevelSkipped",
            Name = "Skipped heading level",
            Description = "Headings should descend one level at a time.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "An H{Level} follows an H{Previous}, skipping a level."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var previous = 0;

            foreach (var heading in facts.Headings)
            {
                // Only a jump downwards matters. Coming back up - h3 then h2 - starts a new
                // section and is perfectly normal.
                if (previous > 0 && heading.Level > previous + 1)
                {
                    context.Report()
                        .Data("Level", heading.Level)
                        .Data("Previous", previous)
                        .Evidence(heading.Text)
                        .Key(heading.Level + ":" + heading.Text);
                    return;
                }

                previous = heading.Level;
            }
        }
    }

    /// <summary>A title repeated verbatim as the H1 wastes one of the two strongest signals.</summary>
    public sealed class H1DuplicatesTitleCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "H1DuplicatesTitle",
            Name = "H1 duplicates the title",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The H1 and the title are identical; between them they could cover more ground."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var h1 = facts.PrimaryH1;
            if (string.IsNullOrWhiteSpace(h1) || string.IsNullOrWhiteSpace(facts.Title)) return;

            if (string.Equals(h1, facts.Title, StringComparison.OrdinalIgnoreCase))
                context.Report().Evidence(h1);
        }
    }

    public sealed class MissingLangCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MissingLangAttribute",
            Name = "Missing lang attribute",
            Description = "The html element should declare the page language.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The html element has no lang attribute."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.Lang)) context.Report();
        }
    }

    public sealed class MissingViewportCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MissingViewportMeta",
            Name = "Missing viewport",
            Description = "Without a viewport meta tag the page will not adapt to a phone screen.",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page has no viewport meta tag."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.Viewport)) context.Report();
        }
    }
}
