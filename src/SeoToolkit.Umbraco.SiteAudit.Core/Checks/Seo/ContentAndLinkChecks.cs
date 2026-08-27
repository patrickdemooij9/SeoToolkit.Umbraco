#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    public sealed class ThinContentCheck : HtmlPageCheckBase
    {
        public const string MinOption = "MinimumWords";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "ThinContent",
            Name = "Thin content",
            Description = "Pages with very little text rarely rank for anything.",
            Category = SeoCheckCategory.Content,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MinOption, "Minimum words", 150, 10, 5000)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Only {Words} words of text, below the {Min} word guideline."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var minimum = context.Options.Get(MinOption, 150);

            if (facts.WordCount < minimum)
                context.Report().Data("Words", facts.WordCount).Data("Min", minimum);
        }
    }

    /// <summary>
    /// Very little text relative to markup usually means a page built almost entirely of
    /// template, with the actual content an afterthought.
    /// </summary>
    public sealed class TextToHtmlRatioCheck : HtmlPageCheckBase
    {
        public const string MinOption = "MinimumPercent";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "LowTextToHtmlRatio",
            Name = "Low text to HTML ratio",
            Category = SeoCheckCategory.Content,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Decimal(MinOption, "Minimum percent", 10, 1, 100)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Text is {Percent}% of the markup."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            // A page with no text at all is thin content's finding, not this one's.
            if (facts.WordCount == 0) return;

            var percent = Math.Round(facts.TextToHtmlRatio * 100, 1);
            var minimum = context.Options.Get(MinOption, 10d);

            if (percent < minimum) context.Report().Data("Percent", percent).Data("Min", minimum);
        }
    }

    /// <summary>
    /// Placeholder text that reached production. Cheap to detect and embarrassing to miss.
    /// </summary>
    public sealed class PlaceholderContentCheck : HtmlPageCheckBase
    {
        public const string PhrasesOption = "Phrases";

        private static readonly string[] DefaultPhrases =
        {
            "lorem ipsum", "dolor sit amet", "todo:", "tbd", "placeholder text", "insert text here"
        };

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "PlaceholderContent",
            Name = "Placeholder content",
            Description = "Draft or filler text left on a published page.",
            Category = SeoCheckCategory.Content,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 5,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.TextList(PhrasesOption, "Phrases", DefaultPhrases)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The text contains \"{Phrase}\"."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrEmpty(facts.TextContent)) return;

            foreach (var phrase in context.Options.Get(PhrasesOption, DefaultPhrases))
            {
                if (facts.TextContent!.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    context.Report().Data("Phrase", phrase).Key(phrase);
                    return;
                }
            }
        }
    }

    /// <summary>A link with no text tells neither a visitor nor a search engine where it goes.</summary>
    public sealed class EmptyAnchorTextCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "EmptyAnchorText",
            Name = "Link with no text",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 4,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The link to {Url} has no text."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var link in facts.Links)
            {
                if (!link.HasEmptyAnchorText) continue;

                context.Report().Data("Url", link.Url.AbsoluteUri).Key(link.Url.AbsoluteUri);
            }
        }
    }

    /// <summary>
    /// "Click here" and friends waste the strongest hint available about the target page.
    /// </summary>
    public sealed class GenericAnchorTextCheck : HtmlPageCheckBase
    {
        public const string PhrasesOption = "Phrases";

        private static readonly string[] DefaultPhrases =
        {
            "click here", "read more", "more", "here", "link", "this page", "learn more"
        };

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "GenericAnchorText",
            Name = "Generic link text",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.TextList(PhrasesOption, "Phrases", DefaultPhrases)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "\"{Text}\" says nothing about where the link goes."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var phrases = context.Options.Get(PhrasesOption, DefaultPhrases);

            foreach (var link in facts.Links)
            {
                if (link.AnchorText is null) continue;

                // Navigation repeats the same wording on every page by design, so judging it here
                // would report the same handful of links across the whole site.
                if (link.Region is LinkRegion.Navigation or LinkRegion.Footer) continue;

                foreach (var phrase in phrases)
                {
                    if (!string.Equals(link.AnchorText, phrase, StringComparison.OrdinalIgnoreCase)) continue;

                    context.Report().Data("Text", link.AnchorText).Key(link.Url.AbsoluteUri);
                    break;
                }
            }
        }
    }

    public sealed class TooManyLinksCheck : HtmlPageCheckBase
    {
        public const string MaxOption = "MaxLinks";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "TooManyLinksOnPage",
            Name = "Too many links",
            Description = "A very large number of links dilutes the value passed to each.",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum links", 150, 10, 5000)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page has {Count} links."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var max = context.Options.Get(MaxOption, 150);

            if (facts.Links.Count > max)
                context.Report().Data("Count", facts.Links.Count).Data("Max", max);
        }
    }

    /// <summary>
    /// A page many clicks from the home page is one both visitors and crawlers reach last, if at all.
    /// </summary>
    public sealed class DeepPageCheck : SeoPageCheckBase
    {
        public const string MaxOption = "MaxDepth";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "DeepPage",
            Name = "Buried page",
            Category = SeoCheckCategory.Links,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum depth", 4, 1, 20)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page is {Depth} clicks from the start of the crawl."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var max = context.Options.Get(MaxOption, 4);

            if (context.Resource.Depth > max)
                context.Report().Data("Depth", context.Resource.Depth).Data("Max", max);
        }
    }
}
