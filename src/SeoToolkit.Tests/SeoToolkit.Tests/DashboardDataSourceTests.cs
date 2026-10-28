using Microsoft.AspNetCore.Mvc;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.PostModels;
using SeoToolkit.Umbraco.Redirects.Core.Dashboard;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Dashboard;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Dashboard;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class DashboardDataSourceTests
    {
        [Test]
        public async Task GetData_UnknownDataSource_ReturnsNotFound()
        {
            // Arrange
            var controller = new DashboardController(new DashboardDataSourceCollection(() => Array.Empty<IDashboardDataSource>()));

            // Act
            var result = await controller.GetData(new DashboardDataPostModel { DataSource = "unknown" });

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetData_KnownDataSource_PassesParametersAndReturnsData()
        {
            // Arrange
            var expected = new DashboardData { Value = 3 };
            var dataSource = new Mock<IDashboardDataSource>();
            dataSource.SetupGet(it => it.Alias).Returns("test.source");
            dataSource.Setup(it => it.GetAsync(It.Is<IReadOnlyDictionary<string, string>>(p => p["checks"] == "BrokenLinkCheck")))
                .ReturnsAsync(expected);
            var controller = new DashboardController(new DashboardDataSourceCollection(() => new[] { dataSource.Object }));

            // Act
            var result = await controller.GetData(new DashboardDataPostModel
            {
                DataSource = "Test.Source",
                Parameters = new Dictionary<string, string> { ["checks"] = "BrokenLinkCheck" }
            });

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.SameAs(expected));
        }

        [Test]
        public void RobotsTxtIndexable_DisallowAllForWildcard_IsNotIndexable()
        {
            var data = RobotsTxtIndexableDataSource.Create("User-agent: *\nDisallow: /\n");

            Assert.That(data.Text, Is.EqualTo("No"));
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Danger));
        }

        [Test]
        public void RobotsTxtIndexable_DisallowAllForOtherAgentOnly_IsIndexable()
        {
            var data = RobotsTxtIndexableDataSource.Create("User-agent: BadBot\nDisallow: /\n\nUser-agent: *\nDisallow: /umbraco # backoffice\n");

            Assert.That(data.Text, Is.EqualTo("Yes"));
            Assert.That(data.Items.Single(it => it.Label == "Rules").Value, Is.EqualTo(2));
        }

        [Test]
        public void RobotsTxtIndexable_SitemapLines_AreListedAndEmptyContentWarnsAboutMissingSitemap()
        {
            var withSitemap = RobotsTxtIndexableDataSource.Create("Sitemap: https://example.com/sitemap.xml\r\n");
            var empty = RobotsTxtIndexableDataSource.Create("");

            Assert.That(withSitemap.Items.Single(it => it.Label == "Sitemap reference").Text, Is.EqualTo("https://example.com/sitemap.xml"));
            Assert.That(empty.Text, Is.EqualTo("Yes"));
            Assert.That(empty.Items.Single(it => it.Label == "Sitemap reference").Status, Is.EqualTo(DashboardDataStatus.Warning));
        }

        [Test]
        public void SiteAuditHealth_WithoutAudit_AsksToRunOne()
        {
            var data = SiteAuditHealthDataSource.Create(null!);

            Assert.That(data.Value, Is.Null);
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Warning));
        }

        [Test]
        public void SiteAuditHealth_ScoresErrorsAsZeroAndWarningsAsHalf()
        {
            // 4 pages: clean, warning, error, error + warning => (1 + 0.5) / 4 = 38
            var data = SiteAuditHealthDataSource.Create(CreateSummary());

            Assert.That(data.Value, Is.EqualTo(38));
            Assert.That(data.Total, Is.EqualTo(100));
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Danger));
            Assert.That(data.Items.Single(it => it.Label == "Missing Title Check").Value, Is.EqualTo(2));
            Assert.That(data.Items.Single(it => it.Label == "Broken Link Check").Status, Is.EqualTo(DashboardDataStatus.Warning));
        }

        [Test]
        public void SiteAuditChecks_CountsDistinctPagesFailingAnyOfTheGivenChecks()
        {
            var data = SiteAuditChecksDataSource.Create(CreateSummary(), new[] { "MissingTitleCheck", "brokenlinkcheck" }, take: 1);

            Assert.That(data.Value, Is.EqualTo(3));
            Assert.That(data.Total, Is.EqualTo(4));
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Danger));
            // One row per check, then the failing pages limited by "take".
            Assert.That(data.Items.Select(it => it.Label), Is.EqualTo(new[] { "Missing Title Check", "Broken Link Check", "/warning" }));
        }

        [Test]
        public void SiteAuditChecks_UnknownCheck_HasNoFailingPages()
        {
            var data = SiteAuditChecksDataSource.Create(CreateSummary(), new[] { "DoesNotExist" }, take: 5);

            Assert.That(data.Value, Is.EqualTo(0));
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Positive));
        }

        [Test]
        public async Task RedirectsUsage_UsesDayParametersAndListsMostRecentHits()
        {
            // Arrange
            var repository = new Mock<IRedirectsRepository>();
            DateTime recentSince = default, staleBefore = default;
            repository.Setup(it => it.GetStatistics(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((recent, stale) => { recentSince = recent; staleBefore = stale; })
                .Returns(new RedirectStatistics { Enabled = 4, RecentlyHit = 1, Stale = 1, NeverHit = 2 });
            repository.Setup(it => it.GetMostRecentlyHit(2)).Returns(new[]
            {
                new Redirect { OldUrl = "/old", NewUrl = "/new", LastHit = new DateTime(2026, 10, 27, 13, 0, 0, DateTimeKind.Utc) }
            });

            // Act
            var data = await new RedirectsUsageDataSource(repository.Object).GetAsync(new Dictionary<string, string>
            {
                ["recentDays"] = "7", ["staleDays"] = "60", ["take"] = "2"
            });

            // Assert
            Assert.That((DateTime.UtcNow - recentSince).TotalDays, Is.EqualTo(7).Within(0.01));
            Assert.That((DateTime.UtcNow - staleBefore).TotalDays, Is.EqualTo(60).Within(0.01));
            Assert.That(data.Value, Is.EqualTo(1));
            Assert.That(data.Total, Is.EqualTo(4));
            Assert.That(data.Status, Is.EqualTo(DashboardDataStatus.Warning));
            Assert.That(data.Items.Last().Label, Is.EqualTo("/old → /new"));
            Assert.That(data.Items.Last().Date, Is.EqualTo(new DateTime(2026, 10, 27, 13, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void SiteCheckService_LoadedConcurrently_ReturnsEachCheckOnce()
        {
            // Arrange: a slow repository widens the window in which requests load the checks at the same time.
            var checks = new[] { CreateCheck(1, "MissingTitleCheck", "Missing Title Check").Check, CreateCheck(2, "BrokenLinkCheck", "Broken Link Check").Check };
            var repository = new Mock<ISiteCheckRepository>();
            repository.Setup(it => it.Get(It.IsAny<string>())).Returns(() => { Thread.Sleep(20); return 1; });
            var settings = new Mock<ISettingsService<SiteAuditConfigModel>>();
            settings.Setup(it => it.GetSettings()).Returns(new SiteAuditConfigModel());
            var service = new SiteCheckService(repository.Object, settings.Object, new SiteAuditCheckCollection(() => checks));

            // Act
            var results = new int[8];
            Parallel.For(0, results.Length, i => results[i] = service.GetAll().Count());

            // Assert
            Assert.That(results, Is.All.EqualTo(2));
        }

        private static SiteAuditResultsSummary CreateSummary()
        {
            var title = CreateCheck(1, "MissingTitleCheck", "Missing Title Check");
            var brokenLink = CreateCheck(2, "BrokenLinkCheck", "Broken Link Check");

            SiteAuditPageResults Page(string url, params (int checkId, SiteCrawlResultType result)[] results) => new()
            {
                Url = url,
                Results = results.Select(it => new SiteAuditPageResult { CheckId = it.checkId, Result = it.result }).ToList()
            };

            return new SiteAuditResultsSummary
            {
                AuditId = 1,
                CreatedDate = new DateTime(2026, 10, 27),
                Checks = new List<SiteCheckDto> { title, brokenLink },
                Pages = new List<SiteAuditPageResults>
                {
                    Page("/clean", (1, SiteCrawlResultType.Success)),
                    Page("/warning", (2, SiteCrawlResultType.Warning)),
                    Page("/error", (1, SiteCrawlResultType.Error)),
                    Page("/both", (1, SiteCrawlResultType.Error), (2, SiteCrawlResultType.Warning))
                }
            };
        }

        private static SiteCheckDto CreateCheck(int id, string alias, string name)
        {
            var check = new Mock<ISiteCheck>();
            check.SetupGet(it => it.Alias).Returns(alias);
            check.SetupGet(it => it.Name).Returns(name);
            return new SiteCheckDto { Id = id, Check = check.Object };
        }
    }
}
