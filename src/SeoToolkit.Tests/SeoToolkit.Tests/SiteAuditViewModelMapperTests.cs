using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Tests
{
    /// <summary>A catalogue backed by whatever descriptors a test hands it.</summary>
    internal sealed class StubCheckCatalogue : ISeoCheckCatalogue
    {
        private readonly Dictionary<string, SeoCheckDescriptor> _descriptors;
        private readonly DefaultSeoCheckMessageFormatter _formatter = new();

        public StubCheckCatalogue(params ISeoCheck[] checks)
        {
            _descriptors = checks.ToDictionary(it => it.Descriptor.Alias, it => it.Descriptor,
                StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> Unavailable { get; } = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<SeoCheckDescriptor> GetAll() => _descriptors.Values.ToArray();

        public SeoCheckDescriptor? Get(string alias)
            => alias is not null && _descriptors.TryGetValue(alias, out var descriptor) ? descriptor : null;

        public bool IsAvailable(SeoCheckDescriptor descriptor) => !Unavailable.Contains(descriptor.Alias);

        public string Describe(AuditIssue issue)
        {
            var descriptor = Get(issue.CheckAlias);
            if (descriptor is null) return issue.Evidence ?? issue.CheckAlias;

            var rebuilt = new SeoCheckIssue(issue.CheckAlias, issue.Severity, issue.Variant, issue.Url);
            return _formatter.Format(rebuilt, descriptor);
        }
    }

    [TestFixture]
    public class SiteAuditViewModelMapperTests
    {
        private static SiteAuditViewModelMapper Mapper(ISeoCheckCatalogue? catalogue = null)
            => new(catalogue ?? new StubCheckCatalogue(new SampleTitleCheck()));

        private static AuditRun Run(SiteAuditStatus status = SiteAuditStatus.Finished) => new()
        {
            Id = 1,
            Key = Guid.NewGuid(),
            Name = "Nightly",
            StartingUrl = new Uri("https://example.com/"),
            Status = status,
            CreatedUtc = DateTime.UtcNow,
            TotalCrawled = 40,
            TotalDiscovered = 40,
            ErrorCount = 3,
            WarningCount = 5,
            CriticalCount = 1
        };

        private static AuditResource Resource(IndexabilityFlags indexability = IndexabilityFlags.Indexable) => new()
        {
            Id = 7,
            Url = new Uri("https://example.com/section/page?x=1"),
            StatusCode = 200,
            Kind = ResourceKind.HtmlPage,
            Indexability = indexability,
            IssueCount = 2,
            ErrorCount = 1,
            WarningCount = 1
        };

        [Test]
        public void MapOverview_CountsCriticalFindingsAsErrors()
        {
            //An editor scanning the list should not have to know the difference; a critical
            //finding hidden from the error column would make a broken site look healthier.
            var model = Mapper().MapOverview(Run());

            Assert.Multiple(() =>
            {
                Assert.That(model.ErrorCount, Is.EqualTo(4));
                Assert.That(model.WarningCount, Is.EqualTo(5));
                Assert.That(model.Status, Is.EqualTo("Finished"));
            });
        }

        [Test]
        public void MapStatus_ReportsProgressAndWhetherTheRunHasSettled()
        {
            var model = Mapper().MapStatus(Run(SiteAuditStatus.Interrupted));

            Assert.Multiple(() =>
            {
                Assert.That(model.IsFinished, Is.True);
                Assert.That(model.Progress, Is.EqualTo(100));
            });
        }

        [Test]
        public void MapDetail_IncludesEverythingExceptResults()
        {
            var model = Mapper().MapDetail(Run());

            Assert.Multiple(() =>
            {
                Assert.That(model.Name, Is.EqualTo("Nightly"));
                Assert.That(model.StartingUrl, Is.EqualTo("https://example.com/"));
                Assert.That(model.TotalCrawled, Is.EqualTo(40));
            });
        }

        [Test]
        public void MapResource_ExposesThePathSeparatelyFromTheFullUrl()
        {
            //The grid shows paths; the full url is what a tooltip and a link need.
            var model = Mapper().MapResource(Resource());

            Assert.Multiple(() =>
            {
                Assert.That(model.Path, Is.EqualTo("/section/page?x=1"));
                Assert.That(model.Url, Is.EqualTo("https://example.com/section/page?x=1"));
            });
        }

        [Test]
        public void MapResource_ListsNoReasonsForAnIndexablePage()
        {
            var model = Mapper().MapResource(Resource());

            Assert.Multiple(() =>
            {
                Assert.That(model.IsIndexable, Is.True);
                Assert.That(model.IndexabilityReasons, Is.Empty);
            });
        }

        [Test]
        public void MapResource_ExplainsEveryReasonAPageCannotBeIndexed()
        {
            //Several causes can apply at once, and an editor needs to know which - being told
            //only that something is wrong is not actionable.
            var model = Mapper().MapResource(
                Resource(IndexabilityFlags.NoIndex | IndexabilityFlags.BlockedByRobotsTxt));

            Assert.Multiple(() =>
            {
                Assert.That(model.IsIndexable, Is.False);
                Assert.That(model.IndexabilityReasons,
                    Is.EquivalentTo(new[] { "NoIndex", "BlockedByRobotsTxt" }));
            });
        }

        [Test]
        public void MapIssue_BuildsTheMessageFromStoredDataAtReadTime()
        {
            var issue = new AuditIssue
            {
                Id = 1,
                CheckAlias = SampleTitleCheck.CheckAlias,
                Variant = "Missing",
                Severity = SeoSeverity.Error,
                Url = new Uri("https://example.com/a")
            };

            var model = Mapper().MapIssue(issue);

            Assert.Multiple(() =>
            {
                Assert.That(model.Message, Is.EqualTo("This page has no title."));
                Assert.That(model.CheckName, Is.EqualTo("Title length"));
                Assert.That(model.Category, Is.EqualTo("OnPage"));
                Assert.That(model.IsError, Is.True);
                Assert.That(model.IsWarning, Is.False);
            });
        }

        [Test]
        public void MapIssue_TreatsCriticalAsAnError()
        {
            var issue = new AuditIssue
            {
                CheckAlias = SampleTitleCheck.CheckAlias,
                Severity = SeoSeverity.Critical
            };

            Assert.That(Mapper().MapIssue(issue).IsError, Is.True);
        }

        [Test]
        public void MapIssue_StillShowsSomethingWhenTheCheckIsNoLongerInstalled()
        {
            //Removing an add-on must not blank out the findings it left behind.
            var issue = new AuditIssue
            {
                CheckAlias = "Gone.Missing",
                Severity = SeoSeverity.Warning,
                Evidence = "Something was wrong here"
            };

            var model = Mapper().MapIssue(issue);

            Assert.Multiple(() =>
            {
                Assert.That(model.CheckName, Is.EqualTo("Gone.Missing"));
                Assert.That(model.Message, Is.EqualTo("Something was wrong here"));
            });
        }

        [Test]
        public void MapSummary_GroupsChecksByCategoryWithTheWorstFirst()
        {
            var checkRuns = new[]
            {
                CheckRun("A", SeoCheckCategory.OnPage, failed: 1, issues: 1),
                CheckRun("B", SeoCheckCategory.Links, failed: 9, issues: 12),
                CheckRun("C", SeoCheckCategory.OnPage, failed: 4, issues: 4)
            };

            var summary = Mapper().MapSummary(checkRuns);

            Assert.Multiple(() =>
            {
                Assert.That(summary.Categories[0].Category, Is.EqualTo("Links"), "most issues first");
                Assert.That(summary.Categories[1].Category, Is.EqualTo("OnPage"));
                Assert.That(summary.Categories[1].IssueCount, Is.EqualTo(5), "summed across its checks");
                Assert.That(summary.Categories[1].Checks[0].Alias, Is.EqualTo("C"),
                    "worst check first within the category");
            });
        }

        [Test]
        public void MapSummary_KeepsChecksThatFoundNothing()
        {
            //A passing check is information: it says the site was examined for that problem.
            var summary = Mapper().MapSummary(new[] { CheckRun("A", SeoCheckCategory.OnPage, 0, 0) });

            Assert.That(summary.Categories.Single().Checks, Has.Count.EqualTo(1));
        }

        [Test]
        public void MapCatalogueEntry_FlagsAChecksAvailabilityAndItsFeature()
        {
            var catalogue = new StubCheckCatalogue(new SampleTitleCheck());
            catalogue.Unavailable.Add(SampleTitleCheck.CheckAlias);

            var entry = Mapper(catalogue).MapCatalogueEntry(new SampleTitleCheck().Descriptor);

            Assert.Multiple(() =>
            {
                Assert.That(entry.IsAvailable, Is.False);
                Assert.That(entry.Alias, Is.EqualTo(SampleTitleCheck.CheckAlias));
                Assert.That(entry.Options.Select(it => it.Key),
                    Is.EquivalentTo(new[] { "MaxLength", "MinLength" }),
                    "declared options travel to the client, so a settings form can be generated");
            });
        }

        private static AuditCheckRun CheckRun(string alias, SeoCheckCategory category, int failed, int issues) => new()
        {
            CheckAlias = alias,
            CheckName = alias,
            Category = category,
            DidRun = true,
            ApplicableCount = 20,
            FailedCount = failed,
            IssueCount = issues,
            Weight = 5,
            Severity = SeoSeverity.Warning
        };
    }

    [TestFixture]
    public class CsvWriterTests
    {
        [Test]
        public void Row_QuotesEveryValue()
        {
            var csv = new CsvWriter().Row("a", "b").ToString();

            Assert.That(csv, Is.EqualTo("\"a\",\"b\"\r\n"));
        }

        [Test]
        public void Row_EscapesEmbeddedQuotes()
        {
            Assert.That(new CsvWriter().Row("say \"hi\"").ToString(), Is.EqualTo("\"say \"\"hi\"\"\"\r\n"));
        }

        [Test]
        public void Row_KeepsCommasAndNewlinesInsideOneField()
        {
            var csv = new CsvWriter().Row("a,b\nc").ToString();

            Assert.That(csv, Is.EqualTo("\"a,b\nc\"\r\n"));
        }

        [TestCase("=cmd|' /c calc'!A0")]
        [TestCase("+1+1")]
        [TestCase("-2+3")]
        [TestCase("@SUM(A1:A9)")]
        public void Escape_NeutralisesSpreadsheetFormulas(string dangerous)
        {
            //Titles and urls come from a crawled site, so an export is untrusted content. A cell
            //starting with one of these is executed when the file is opened.
            var escaped = CsvWriter.Escape(dangerous);

            Assert.That(escaped, Does.StartWith("\"'"));
        }

        [Test]
        public void Escape_LeavesOrdinaryTextAlone()
        {
            Assert.That(CsvWriter.Escape("https://example.com/a"), Is.EqualTo("\"https://example.com/a\""));
        }

        [Test]
        public void Escape_HandlesNullAndEmpty()
        {
            Assert.Multiple(() =>
            {
                Assert.That(CsvWriter.Escape(null), Is.EqualTo("\"\""));
                Assert.That(CsvWriter.Escape(string.Empty), Is.EqualTo("\"\""));
            });
        }
    }
}
