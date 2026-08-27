using System.Collections.Concurrent;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// Checks written purely to exercise the public check api the way an add-on package would.
    /// They are deliberately realistic rather than minimal: if writing one of these is awkward,
    /// the api is wrong.
    /// </summary>
    internal sealed class SampleTitleCheck : HtmlPageCheckBase
    {
        public const string CheckAlias = "Sample.Title";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = CheckAlias,
            Name = "Title length",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 8,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int("MaxLength", "Maximum length", 60, 10, 200),
                SeoCheckOption.Int("MinLength", "Minimum length", 15, 1, 200)),
            MessageTemplates = new Dictionary<string, string>
            {
                ["Missing"] = "This page has no title.",
                ["TooLong"] = "Title is {Length} characters, maximum is {Max}.",
                ["TooShort"] = "Title is only {Length} characters, minimum is {Min}."
            }
        };

        protected override void Check(PageFacts facts, in SeoPageCheckContext context)
        {
            if (string.IsNullOrWhiteSpace(facts.Title))
            {
                context.Report("Missing", SeoSeverity.Error);
                return;
            }

            var max = context.Options.Get("MaxLength", 60);
            var min = context.Options.Get("MinLength", 15);

            if (facts.Title!.Length > max)
                context.Report("TooLong").Data("Length", facts.Title.Length).Data("Max", max).Evidence(facts.Title);
            else if (facts.Title.Length < min)
                context.Report("TooShort").Data("Length", facts.Title.Length).Data("Min", min).Evidence(facts.Title);
        }
    }

    /// <summary>A site-wide check that works purely off the shared crawl index.</summary>
    internal sealed class SampleDuplicateTitleCheck : SeoSiteCheckBase
    {
        public const string CheckAlias = "Sample.DuplicateTitle";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = CheckAlias,
            Name = "Duplicate titles",
            Category = SeoCheckCategory.OnPage,
            DefaultSeverity = SeoSeverity.Warning,
            Weight = 7,
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "{Count} pages share the title \"{Title}\"."
            }
        };

        protected override void Evaluate(SeoSiteCheckContext context)
        {
            foreach (var group in context.Index.DuplicatesBy(DuplicateFacet.Title))
            {
                foreach (var page in group)
                {
                    var issue = context.Report(page.Url)
                        .Data("Title", page.Title!)
                        .Data("Count", group.Count)
                        .Evidence(page.Title);

                    foreach (var other in group)
                    {
                        if (!ReferenceEquals(other, page))
                            issue.Related(other.Url);
                    }
                }
            }
        }
    }

    /// <summary>Per-run state for <see cref="SampleSlowPageCheck"/>.</summary>
    internal sealed class SlowPageCollector : ISeoCrawlCollector
    {
        private readonly ConcurrentBag<(Uri Url, int Ms)> _slow = new();
        private int _observed;

        public int Observed => _observed;

        public IReadOnlyList<(Uri Url, int Ms)> Slow => _slow.ToArray();

        public void Observe(in SeoPageCheckContext context)
        {
            Interlocked.Increment(ref _observed);

            var threshold = context.Options.Get("ThresholdMs", 2000);
            if (context.Resource.TotalMs >= threshold)
                _slow.Add((context.Resource.RequestedUrl, context.Resource.TotalMs));
        }
    }

    /// <summary>
    /// A site-wide check that accumulates its own state during the crawl, proving the
    /// collector path rather than the index path.
    /// </summary>
    internal sealed class SampleSlowPageCheck : SeoSiteCheckBase<SlowPageCollector>
    {
        public const string CheckAlias = "Sample.SlowPages";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = CheckAlias,
            Name = "Slow pages",
            Category = SeoCheckCategory.Performance,
            DefaultSeverity = SeoSeverity.Warning,
            Options = SeoCheckDescriptor.OptionsFrom(
                SeoCheckOption.Int("ThresholdMs", "Slow above (ms)", 2000, 100, 60000)),
            MessageTemplates = new Dictionary<string, string>
            {
                [""] = "Responded in {Ms} ms."
            }
        };

        protected override void Evaluate(SlowPageCollector collector, SeoSiteCheckContext context)
        {
            foreach (var (url, ms) in collector.Slow)
                context.Report(url).Data("Ms", ms);
        }
    }

    /// <summary>A check that has to await something, to prove the async path.</summary>
    internal sealed class SampleAsyncCheck : SeoPageCheckBase
    {
        public const string CheckAlias = "Sample.Async";

        protected override SeoCheckDescriptor CreateDescriptor() => new()
        {
            Alias = CheckAlias,
            Name = "Async sample",
            Category = SeoCheckCategory.Links,
            RequiredCapabilities = SeoCheckCapabilities.ExternalRequests
        };

        public override async ValueTask RunAsync(SeoPageCheckContext context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            if (context.Resource.StatusCode >= 400)
                context.Report("BadStatus").Data("StatusCode", context.Resource.StatusCode);
        }
    }
}
