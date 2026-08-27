using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SeoCheckApiTests
    {
        [Test]
        public void PageCheck_WhenNothingIsWrong_ReportsNothing()
        {
            var issues = new SampleTitleCheck().Run(
                CrawledResourceBuilder.A().Title("A perfectly reasonable page title").Build());

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void PageCheck_ReportsVariantAndStructuredData_RatherThanAFormattedSentence()
        {
            var title = new string('x', 90);

            var issues = new SampleTitleCheck().Run(CrawledResourceBuilder.A().Title(title).Build());

            Assert.That(issues, Has.Count.EqualTo(1));
            var issue = issues[0];
            Assert.Multiple(() =>
            {
                Assert.That(issue.Variant, Is.EqualTo("TooLong"));
                Assert.That(issue.Data["Length"], Is.EqualTo("90"));
                Assert.That(issue.Data["Max"], Is.EqualTo("60"));
                Assert.That(issue.Evidence, Is.EqualTo(title));
                Assert.That(issue.CheckAlias, Is.EqualTo(SampleTitleCheck.CheckAlias));
            });
        }

        [Test]
        public void PageCheck_CanOverrideSeverityPerIssue()
        {
            var issues = new SampleTitleCheck().Run(CrawledResourceBuilder.A().Title(null).Build());

            Assert.Multiple(() =>
            {
                Assert.That(issues[0].Variant, Is.EqualTo("Missing"));
                //Descriptor default is Warning; the check escalated this particular case.
                Assert.That(issues[0].Severity, Is.EqualTo(SeoSeverity.Error));
            });
        }

        [Test]
        public void PageCheck_HonoursOptionOverrides()
        {
            var check = new SampleTitleCheck();
            var resource = CrawledResourceBuilder.A().Title(new string('x', 70)).Build();

            var withDefaults = check.Run(resource);
            var withRaisedLimit = check.Run(resource,
                optionOverrides: new Dictionary<string, object> { ["MaxLength"] = 100 });

            Assert.Multiple(() =>
            {
                Assert.That(withDefaults, Has.Count.EqualTo(1), "70 chars exceeds the default 60");
                Assert.That(withRaisedLimit, Is.Empty, "70 chars is within the overridden 100");
            });
        }

        [Test]
        public void HtmlPageCheck_SkipsResourcesWithNoParsedMarkup()
        {
            //A failed fetch is someone else's finding to report, not the title check's.
            var issues = new SampleTitleCheck().Run(CrawledResourceBuilder.A().NoFacts().Build());

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public async Task PageCheck_SupportsAnAsynchronousBody()
        {
            var check = new SampleAsyncCheck();
            var sink = new RecordingIssueSink();
            var context = new SeoPageCheckContext(
                CrawledResourceBuilder.A().Status(503).Build(),
                check.Descriptor,
                SeoCheckOptions.Empty,
                new FakeRunContext(),
                sink);

            await check.RunAsync(context, CancellationToken.None);

            Assert.That(sink.Issues, Has.Count.EqualTo(1));
            Assert.That(sink.Issues[0].Data["StatusCode"], Is.EqualTo("503"));
        }

        [Test]
        public void SiteCheck_FindsDuplicatesAcrossPages_WhichPerPageChecksCannotDo()
        {
            var index = new CrawlIndex();
            AddPage(index, "https://example.com/a", title: "Shared title");
            AddPage(index, "https://example.com/b", title: "Shared title");
            AddPage(index, "https://example.com/c", title: "Something else");

            var issues = new SampleDuplicateTitleCheck().RunOver(index, Array.Empty<CrawledResource>());

            Assert.Multiple(() =>
            {
                Assert.That(issues, Has.Count.EqualTo(2), "one issue per page in the duplicate group");
                Assert.That(issues.Select(it => it.Url!.AbsoluteUri),
                    Is.EquivalentTo(new[] { "https://example.com/a", "https://example.com/b" }));
                Assert.That(issues[0].Data["Count"], Is.EqualTo("2"));
                Assert.That(issues[0].RelatedUrls, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void SiteCheck_WithACollector_AccumulatesDuringTheCrawl()
        {
            var index = new CrawlIndex();
            var resources = new[]
            {
                Timed("https://example.com/fast", 120),
                Timed("https://example.com/slow", 4200),
                Timed("https://example.com/alsoslow", 2600)
            };

            var issues = new SampleSlowPageCheck().RunOver(index, resources);

            Assert.That(issues.Select(it => it.Url!.AbsoluteUri),
                Is.EquivalentTo(new[] { "https://example.com/slow", "https://example.com/alsoslow" }));
        }

        [Test]
        public void SiteCheck_CollectorHonoursOptions()
        {
            var index = new CrawlIndex();
            var resources = new[] { Timed("https://example.com/slow", 2600) };

            var issues = new SampleSlowPageCheck().RunOver(index, resources,
                optionOverrides: new Dictionary<string, object> { ["ThresholdMs"] = 3000 });

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void SiteCheck_AlwaysRequiresTheCrawlIndex_EvenIfItForgotToSaySo()
        {
            //The sample descriptor never declares CrawlIndex. Forgetting it would only mean the
            //crawler skipped building the index, so the base class adds it rather than let that
            //be a silent bug in every third party site check.
            var descriptor = new SampleDuplicateTitleCheck().Descriptor;

            Assert.That(descriptor.RequiredCapabilities.Requires(SeoCheckCapabilities.CrawlIndex), Is.True);
        }

        [Test]
        public void Descriptor_IsBuiltOnceAndCached()
        {
            var check = new SampleTitleCheck();

            var first = check.Descriptor;
            var second = check.Descriptor;

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Capabilities_AggregateToWhatTheCrawlerMustProvide()
        {
            var combined = SeoCheckCapabilitiesExtensions.Aggregate(new[]
            {
                new SampleTitleCheck().Descriptor,
                new SampleAsyncCheck().Descriptor,
                new SampleDuplicateTitleCheck().Descriptor
            });

            Assert.Multiple(() =>
            {
                Assert.That(combined.Requires(SeoCheckCapabilities.ExternalRequests), Is.True);
                Assert.That(combined.Requires(SeoCheckCapabilities.CrawlIndex), Is.True);
                //Nothing asked for raw bodies, so the crawler is free not to retain them.
                Assert.That(combined.Requires(SeoCheckCapabilities.RawBody), Is.False);
                Assert.That(combined.Requires(SeoCheckCapabilities.RenderedDom), Is.False);
            });
        }

        [Test]
        public void IssueHash_KeepsSeveralFindingsOnOnePageDistinct()
        {
            //A single page with two broken links must produce two identities, not one.
            //This is what the old ISiteCheck.Compare was supposed to do and never did.
            var issues = ReportBrokenLinks("https://other.com/a", "https://other.com/b");

            Assert.That(issues[0].IssueHash, Is.Not.EqualTo(issues[1].IssueHash));
        }

        [Test]
        public void IssueHash_IsStableAcrossRunsForTheSameFinding()
        {
            var first = ReportBrokenLinks("https://other.com/a");
            var second = ReportBrokenLinks("https://other.com/a");

            Assert.That(first[0].IssueHash, Is.EqualTo(second[0].IssueHash));
        }

        private static IReadOnlyList<SeoCheckIssue> ReportBrokenLinks(params string[] brokenUrls)
        {
            var descriptor = new SeoCheckDescriptor
            {
                Alias = "Sample.BrokenLink",
                Name = "Broken link",
                Category = SeoCheckCategory.Links,
                DefaultSeverity = SeoSeverity.Error
            };

            var sink = new RecordingIssueSink();
            var context = new SeoPageCheckContext(
                CrawledResourceBuilder.A().Url("https://example.com/page").Build(),
                descriptor,
                SeoCheckOptions.Empty,
                new FakeRunContext(),
                sink);

            foreach (var brokenUrl in brokenUrls)
                context.Report("Broken").Key(brokenUrl).Data("Url", brokenUrl);

            return sink.Issues;
        }

        private static CrawledResource Timed(string url, int ms) => new()
        {
            RequestedUrl = new Uri(url),
            FinalUrl = new Uri(url),
            NormalizedUrl = url,
            UrlHash = url.GetHashCode().ToString("x8"),
            StatusCode = 200,
            Kind = ResourceKind.HtmlPage,
            IsInternal = true,
            TotalMs = ms
        };

        /// <summary>
        /// Adds a page the way the crawler will: the index allocates the id, so a url that was
        /// already linked to keeps the identity it was given then. Returns that id.
        /// </summary>
        internal static int AddPage(CrawlIndex index, string url, string? title = null,
            string? description = null, string? h1 = null, long contentHash = 0,
            int statusCode = 200, bool isInternal = true,
            ResourceKind kind = ResourceKind.HtmlPage,
            IReadOnlyCollection<string>? outlinks = null)
        {
            var id = index.GetOrCreateId(url);
            index.Add(new ResourceSummary
            {
                Id = id,
                NormalizedUrl = url,
                Url = new Uri(url),
                StatusCode = statusCode,
                Kind = kind,
                IsInternal = isInternal,
                Title = title,
                MetaDescription = description,
                H1 = h1,
                ContentHash = contentHash
            }, outlinks);
            return id;
        }
    }
}
