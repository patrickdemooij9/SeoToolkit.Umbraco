#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>
    /// An image with no alt attribute at all. An empty alt is deliberate - it marks a decorative
    /// image - so the two are treated differently rather than lumped together.
    /// </summary>
    public sealed class ImageAltCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MissingImageAlt",
            Name = "Image without alt text",
            Description = "Alt text describes an image to search engines and screen readers.",
            Category = SeoCheckCategory.Images,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Url} has no alt attribute."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var image in facts.Images)
            {
                if (image.HasAltAttribute) continue;

                context.Report().Data("Url", image.Url.AbsoluteUri).Key(image.Url.AbsoluteUri);
            }
        }
    }

    /// <summary>
    /// An image with no width and height makes the page jump around as it loads, which is both
    /// unpleasant and a measured ranking signal.
    /// </summary>
    public sealed class ImageDimensionsCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "ImageMissingDimensions",
            Name = "Image without dimensions",
            Description = "Width and height let the browser reserve space before the image loads.",
            Category = SeoCheckCategory.Images,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Url} has no width and height, so the layout shifts as it loads."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var image in facts.Images)
            {
                if (image.HasDimensions) continue;

                context.Report().Data("Url", image.Url.AbsoluteUri).Key(image.Url.AbsoluteUri);
            }
        }
    }

    /// <summary>Modern formats are markedly smaller for the same quality.</summary>
    public sealed class ImageFormatCheck : HtmlPageCheckBase
    {
        private static readonly string[] Dated = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "ImageNotNextGenFormat",
            Name = "Dated image format",
            Category = SeoCheckCategory.Images,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Url} could be a good deal smaller as WebP or AVIF."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var image in facts.Images)
            {
                var path = image.Url.AbsolutePath;

                foreach (var extension in Dated)
                {
                    if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) continue;

                    context.Report().Data("Url", image.Url.AbsoluteUri).Key(image.Url.AbsoluteUri);
                    break;
                }
            }
        }
    }

    public sealed class InvalidJsonLdCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "InvalidJsonLd",
            Name = "Invalid structured data",
            Description = "Structured data that does not parse is ignored entirely.",
            Category = SeoCheckCategory.StructuredData,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 6,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "A JSON-LD block could not be parsed: {Error}"
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var block in facts.JsonLdBlocks)
            {
                if (block.IsValid) continue;

                context.Report()
                    .Data("Error", block.ParseError ?? "unknown")
                    .Evidence(Excerpt(block.Raw))
                    .Key(block.Raw.GetHashCode().ToString("x8"));
            }
        }

        /// <summary>Enough of the block to recognise it, without putting a whole document in a cell.</summary>
        private static string Excerpt(string raw)
            => raw.Length <= 200 ? raw : raw[..200] + "...";
    }

    public sealed class OpenGraphCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "OpenGraphMissing",
            Name = "Missing Open Graph tags",
            Description = "Without these, links shared to social platforms show no preview.",
            Category = SeoCheckCategory.StructuredData,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                ["Missing"] = "This page has no Open Graph tags.",
                ["NoImage"] = "Open Graph tags are present but there is no og:image."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.OpenGraph.IsEmpty)
            {
                context.Report("Missing");
                return;
            }

            if (facts.OpenGraph.Image is null) context.Report("NoImage");
        }
    }

    public sealed class TwitterCardCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "TwitterCardMissing",
            Name = "Missing Twitter card",
            Category = SeoCheckCategory.StructuredData,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 1,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page has no Twitter card tags."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            // Open Graph is used as a fallback by most platforms, so a page with it is not
            // actually without a preview - which makes this worth reporting only when both are absent.
            if (!facts.TwitterCard.IsEmpty || !facts.OpenGraph.IsEmpty) return;

            context.Report();
        }
    }

    public sealed class FaviconCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MissingFavicon",
            Name = "Missing favicon",
            Category = SeoCheckCategory.Images,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 1,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page declares no favicon."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.FaviconHref is null) context.Report();
        }
    }

    /// <summary>
    /// An hreflang value has to be a language, optionally with a region - "en", "en-GB" - or the
    /// special x-default. Anything else is silently ignored.
    /// </summary>
    public sealed class HreflangCodeCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "HreflangInvalidCode",
            Name = "Invalid hreflang",
            Category = SeoCheckCategory.International,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 5,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "\"{Code}\" is not a valid hreflang value."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            foreach (var entry in facts.HreflangEntries)
            {
                if (entry.IsXDefault || IsValid(entry.Language)) continue;

                context.Report().Data("Code", entry.Language).Key(entry.Language);
            }
        }

        private static bool IsValid(string language)
        {
            var parts = language.Split('-');
            if (parts.Length is < 1 or > 3) return false;

            // Language is two or three letters; the region, when present, is two letters or a
            // three digit UN M49 code.
            if (parts[0].Length is < 2 or > 3 || !IsAlpha(parts[0])) return false;

            for (var i = 1; i < parts.Length; i++)
            {
                var part = parts[i];
                var ok = (part.Length == 2 && IsAlpha(part))
                         || (part.Length == 3 && IsNumeric(part))
                         || (part.Length == 4 && IsAlpha(part));

                if (!ok) return false;
            }

            return true;
        }

        private static bool IsAlpha(string value)
        {
            foreach (var c in value)
                if (!char.IsLetter(c)) return false;
            return true;
        }

        private static bool IsNumeric(string value)
        {
            foreach (var c in value)
                if (!char.IsDigit(c)) return false;
            return true;
        }
    }

    /// <summary>
    /// A set of hreflang annotations should nominate a fallback for visitors whose language is
    /// not covered by any of them.
    /// </summary>
    public sealed class HreflangXDefaultCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "HreflangMissingXDefault",
            Name = "Missing hreflang x-default",
            Category = SeoCheckCategory.International,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 3,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "This page lists {Count} languages but no x-default fallback."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (facts.HreflangEntries.Count == 0) return;

            foreach (var entry in facts.HreflangEntries)
                if (entry.IsXDefault) return;

            context.Report().Data("Count", facts.HreflangEntries.Count);
        }
    }
}
