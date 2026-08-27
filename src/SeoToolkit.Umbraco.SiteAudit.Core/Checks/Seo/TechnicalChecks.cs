#nullable enable
using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo
{
    /// <summary>A linked url that returns 4xx is a dead end for visitors and crawlers alike.</summary>
    public sealed class ClientErrorCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "HttpClientError",
            Name = "Page not found",
            Description = "The url returned a 4xx response.",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 9,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string> { [""] = "Returned {StatusCode}." }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            if (context.Resource.IsClientError)
                context.Report().Data("StatusCode", context.Resource.StatusCode);
        }
    }

    public sealed class ServerErrorCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "HttpServerError",
            Name = "Server error",
            Description = "The url returned a 5xx response.",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Critical,
            Weight = 10,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string> { [""] = "Returned {StatusCode}." }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            if (context.Resource.IsServerError)
                context.Report().Data("StatusCode", context.Resource.StatusCode);
        }
    }

    /// <summary>A url that could not be reached at all - dns, timeout, certificate.</summary>
    public sealed class UnreachableCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "Unreachable",
            Name = "Could not be reached",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Critical,
            Weight = 10,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string> { [""] = "{Reason}: {Detail}" }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var resource = context.Resource;
            if (!resource.IsFailed) return;

            context.Report()
                .Data("Reason", resource.Failure!.Value.ToString())
                .Data("Detail", resource.FailureDetail ?? string.Empty)
                .Evidence(resource.FailureDetail);
        }
    }

    /// <summary>
    /// Every hop costs time and dilutes the signal passed on. One is normal, several is a chain
    /// that should be collapsed.
    /// </summary>
    public sealed class RedirectChainCheck : SeoPageCheckBase
    {
        public const string MaxOption = "MaxHops";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "RedirectChainTooLong",
            Name = "Redirect chain",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 5,
            AppliesTo = ResourceKinds.All,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum hops", 1, 1, 10)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Reached after {Hops} redirects; link straight to {Final}."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var hops = context.Resource.RedirectChain.Count;
            if (hops <= context.Options.Get(MaxOption, 1)) return;

            context.Report()
                .Data("Hops", hops)
                .Data("Final", context.Resource.FinalUrl.AbsoluteUri);
        }
    }

    public sealed class RedirectLoopCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "RedirectLoop",
            Name = "Redirect loop",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Critical,
            Weight = 9,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The redirects loop back on themselves, so this url can never be reached."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            if (context.Resource.IsRedirectLoop) context.Report();
        }
    }

    /// <summary>
    /// A 302 says "this has moved for now". Left in place permanently it keeps the old url as the
    /// one that ranks, which is rarely what was meant.
    /// </summary>
    public sealed class TemporaryRedirectCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "TemporaryRedirect",
            Name = "Temporary redirect",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 4,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Redirected with {StatusCode}; use 301 if the move is permanent."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            foreach (var hop in context.Resource.RedirectChain)
            {
                if (hop.IsPermanent) continue;

                context.Report()
                    .Data("StatusCode", hop.StatusCode)
                    .Evidence($"{hop.From.AbsoluteUri} -> {hop.To.AbsoluteUri}")
                    .Key(hop.From.AbsoluteUri);
                return;
            }
        }
    }

    public sealed class NonHttpsCheck : SeoPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "NonHttpsPage",
            Name = "Not served over HTTPS",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 8,
            AppliesTo = ResourceKinds.All,
            MessageTemplates = new Dictionary<string, string> { [""] = "This url is served over plain HTTP." }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            if (!context.Resource.IsInternal) return;

            if (context.Resource.FinalUrl.Scheme == Uri.UriSchemeHttp) context.Report();
        }
    }

    /// <summary>
    /// A secure page pulling an asset over plain HTTP. Browsers block these outright, so the page
    /// is usually visibly broken as well.
    /// </summary>
    public sealed class MixedContentCheck : HtmlPageCheckBase
    {
        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "MixedContent",
            Name = "Mixed content",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Error,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "A secure page loads {Url} over plain HTTP."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (context.Resource.FinalUrl.Scheme != Uri.UriSchemeHttps) return;

            foreach (var url in Insecure(facts))
            {
                context.Report().Data("Url", url.AbsoluteUri).Key(url.AbsoluteUri).Evidence(url.AbsoluteUri);
            }
        }

        private static IEnumerable<Uri> Insecure(PageFacts facts)
        {
            foreach (var image in facts.Images)
                if (image.Url.Scheme == Uri.UriSchemeHttp) yield return image.Url;

            foreach (var script in facts.Scripts)
                if (script.Scheme == Uri.UriSchemeHttp) yield return script;

            foreach (var stylesheet in facts.Stylesheets)
                if (stylesheet.Scheme == Uri.UriSchemeHttp) yield return stylesheet;
        }
    }

    /// <summary>Replaces the old page performance check, with the thresholds now configurable.</summary>
    public sealed class SlowResponseCheck : SeoPageCheckBase
    {
        public const string SlowOption = "SlowMs";
        public const string VerySlowOption = "VerySlowMs";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "SlowResponse",
            Name = "Slow response",
            Description = "How long the server took to respond.",
            Category = SeoCheckCategory.Performance,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 6,
            AppliesTo = ResourceKinds.All,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(SlowOption, "Slow above (ms)", 2500, 100, 60000),
                SeoCheckOption.Int(VerySlowOption, "Very slow above (ms)", 4000, 100, 60000)),
            MessageTemplates = new Dictionary<string, string>
            {
                ["Slow"] = "Responded in {Ms} ms.",
                ["VerySlow"] = "Responded in {Ms} ms, which visitors will notice."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var elapsed = context.Resource.TotalMs;
            if (elapsed <= 0) return;

            var verySlow = context.Options.Get(VerySlowOption, 4000);
            var slow = context.Options.Get(SlowOption, 2500);

            if (elapsed >= verySlow)
                context.Report("VerySlow", SeoSeverity.Error).Data("Ms", elapsed);
            else if (elapsed >= slow)
                context.Report("Slow").Data("Ms", elapsed);
        }
    }

    /// <summary>A very heavy page is slow on a phone however fast the server is.</summary>
    public sealed class LargePageCheck : HtmlPageCheckBase
    {
        public const string MaxOption = "MaxKilobytes";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "LargeHtmlPage",
            Name = "Large page",
            Category = SeoCheckCategory.Performance,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 4,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum size (KB)", 500, 50, 10000)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The markup is {Kilobytes} KB, over the {Max} KB guideline."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            var kilobytes = facts.HtmlSizeBytes / 1024;
            var max = context.Options.Get(MaxOption, 500);

            if (kilobytes > max)
                context.Report().Data("Kilobytes", kilobytes).Data("Max", max);
        }
    }

    /// <summary>Very long urls are awkward to share and often a sign of a structural problem.</summary>
    public sealed class UrlLengthCheck : SeoPageCheckBase
    {
        public const string MaxOption = "MaxLength";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = "UrlTooLong",
            Name = "Long url",
            Category = SeoCheckCategory.Technical,
            DefaultSeverity = SeoSeverity.Notice,
            Weight = 2,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int(MaxOption, "Maximum length", 115, 50, 2000)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "The url is {Length} characters."
            }
        };

        protected override void Check(in SeoPageCheckContext context)
        {
            var length = context.Resource.RequestedUrl.AbsoluteUri.Length;
            var max = context.Options.Get(MaxOption, 115);

            if (length > max) context.Report().Data("Length", length).Data("Max", max);
        }
    }
}
