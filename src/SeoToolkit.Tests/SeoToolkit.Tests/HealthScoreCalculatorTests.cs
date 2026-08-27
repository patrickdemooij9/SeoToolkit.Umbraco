using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Scoring;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// These assert the score's <em>properties</em> rather than particular numbers. A score that
    /// moves for reasons unrelated to the site is worse than no score, so the invariants matter
    /// more than any single value - and pinning exact numbers would make the formula unchangeable.
    /// </summary>
    [TestFixture]
    public class HealthScoreCalculatorTests
    {
        private static readonly HealthScoreCalculator Calculator = new();

        private static AuditCheckRun Check(string alias,
            SeoCheckCategory category = SeoCheckCategory.OnPage,
            SeoSeverity severity = SeoSeverity.Error,
            int applicable = 100,
            int failed = 0,
            int weight = 5,
            bool didRun = true) => new()
        {
            CheckAlias = alias,
            CheckName = alias,
            Category = category,
            Severity = severity,
            ApplicableCount = applicable,
            FailedCount = failed,
            IssueCount = failed,
            Weight = weight,
            DidRun = didRun
        };

        [Test]
        public void APerfectSiteScoresOneHundred()
        {
            var score = Calculator.Calculate(new[]
            {
                Check("A", failed: 0),
                Check("B", SeoCheckCategory.Technical, failed: 0)
            });

            Assert.Multiple(() =>
            {
                Assert.That(score.HasScore, Is.True);
                Assert.That(score.Score, Is.EqualTo(100));
            });
        }

        [Test]
        public void TheScoreIsNotAffectedByHowManyPagesWereCrawled()
        {
            //Built from ratios. Crawling ten times more of the same site must not move the number,
            //or a bigger crawl would look like a worse site.
            var small = Calculator.Calculate(new[] { Check("A", applicable: 10, failed: 2) });
            var large = Calculator.Calculate(new[] { Check("A", applicable: 1000, failed: 200) });

            Assert.That(large.Score, Is.EqualTo(small.Score));
        }

        [Test]
        public void ACheckThatDidNotRunLeavesTheCalculationEntirely()
        {
            //Not counted as a pass, and not counted as a failure. Otherwise installing an add-on
            //that adds checks would change the score of a site that had not changed at all.
            var withoutIt = Calculator.Calculate(new[] { Check("A", failed: 10) });
            var withSkipped = Calculator.Calculate(new[]
            {
                Check("A", failed: 10),
                Check("Skipped", didRun: false)
            });

            Assert.That(withSkipped.Score, Is.EqualTo(withoutIt.Score));
        }

        [Test]
        public void ACheckNothingWasApplicableToIsIgnored()
        {
            //A site with no images tells us nothing about its image checks either way.
            var baseline = Calculator.Calculate(new[] { Check("A", failed: 10) });
            var withEmpty = Calculator.Calculate(new[]
            {
                Check("A", failed: 10),
                Check("NoImages", SeoCheckCategory.Images, applicable: 0)
            });

            Assert.That(withEmpty.Score, Is.EqualTo(baseline.Score));
        }

        [Test]
        public void MoreFailuresNeverImprovesTheScore()
        {
            var previous = 101;

            foreach (var failed in new[] { 0, 10, 25, 50, 75, 100 })
            {
                var score = Calculator.Calculate(new[] { Check("A", failed: failed) }).Score;

                Assert.That(score, Is.LessThanOrEqualTo(previous), $"at {failed} failures");
                previous = score;
            }
        }

        [Test]
        public void AMoreSevereCheckCostsMore()
        {
            var notice = Calculator.Calculate(new[] { Check("A", severity: SeoSeverity.Notice, failed: 50) });
            var warning = Calculator.Calculate(new[] { Check("A", severity: SeoSeverity.Warning, failed: 50) });
            var error = Calculator.Calculate(new[] { Check("A", severity: SeoSeverity.Error, failed: 50) });
            var critical = Calculator.Calculate(new[] { Check("A", severity: SeoSeverity.Critical, failed: 50) });

            Assert.That(new[] { critical.Score, error.Score, warning.Score, notice.Score },
                Is.Ordered.Ascending);
        }

        [Test]
        public void AHeavierCheckMovesTheCategoryMore()
        {
            var light = Calculator.Calculate(new[]
            {
                Check("Failing", failed: 100, weight: 1),
                Check("Passing", failed: 0, weight: 9)
            });

            var heavy = Calculator.Calculate(new[]
            {
                Check("Failing", failed: 100, weight: 9),
                Check("Passing", failed: 0, weight: 1)
            });

            Assert.That(heavy.Score, Is.LessThan(light.Score));
        }

        [Test]
        public void OneNoisyCategoryCannotDominateTheWholeScore()
        {
            //Categories are weighted independently, so a category failing outright still leaves
            //most of the score standing.
            var score = Calculator.Calculate(new[]
            {
                Check("Broken", SeoCheckCategory.Images, SeoSeverity.Critical, failed: 100),
                Check("FineA", SeoCheckCategory.Indexability, failed: 0),
                Check("FineB", SeoCheckCategory.Technical, failed: 0),
                Check("FineC", SeoCheckCategory.OnPage, failed: 0),
                Check("FineD", SeoCheckCategory.Content, failed: 0)
            });

            Assert.That(score.Score, Is.GreaterThan(50));
        }

        [Test]
        public void AnIndexabilityFailureCostsMoreThanTheSameFailureElsewhere()
        {
            //A page search engines cannot index is worth nothing however well written it is.
            var indexability = Calculator.Calculate(new[]
            {
                Check("Bad", SeoCheckCategory.Indexability, failed: 100),
                Check("Fine", SeoCheckCategory.Other, failed: 0)
            });

            var other = Calculator.Calculate(new[]
            {
                Check("Bad", SeoCheckCategory.Other, failed: 100),
                Check("Fine", SeoCheckCategory.Indexability, failed: 0)
            });

            Assert.That(indexability.Score, Is.LessThan(other.Score));
        }

        [Test]
        public void TheScoreStaysWithinRange()
        {
            var worst = Calculator.Calculate(new[]
            {
                Check("A", SeoCheckCategory.Indexability, SeoSeverity.Critical, failed: 100),
                Check("B", SeoCheckCategory.Technical, SeoSeverity.Critical, failed: 100),
                Check("C", SeoCheckCategory.OnPage, SeoSeverity.Critical, failed: 100)
            });

            Assert.That(worst.Score, Is.InRange(0, 100));
        }

        [Test]
        public void EverythingCriticalAndEverythingFailingScoresZero()
        {
            var score = Calculator.Calculate(new[]
            {
                Check("A", SeoCheckCategory.Indexability, SeoSeverity.Critical, failed: 100)
            });

            Assert.That(score.Score, Is.Zero);
        }

        [Test]
        public void ARunWithNothingMeasurableHasNoScoreAtAll()
        {
            //Showing zero here would say the site is broken, when in fact nothing was measured.
            var score = Calculator.Calculate(Array.Empty<AuditCheckRun>());

            Assert.Multiple(() =>
            {
                Assert.That(score.HasScore, Is.False);
                Assert.That(score.Categories, Is.Empty);
            });
        }

        [Test]
        public void MoreFailuresThanApplicableResourcesDoesNotPushTheScoreNegative()
        {
            //Defensive: the counts come from the crawl, and a bad one should degrade rather than
            //produce a nonsensical score.
            var score = Calculator.Calculate(new[] { Check("A", applicable: 10, failed: 999) });

            Assert.That(score.Score, Is.InRange(0, 100));
        }

        [Test]
        public void TheBreakdownExplainsWhereTheScoreWentAndIsOrderedWorstFirst()
        {
            //"Why is my score 72" has to be answerable, or the number is just decoration.
            var score = Calculator.Calculate(new[]
            {
                Check("Clean", failed: 0, weight: 5),
                Check("Broken", failed: 80, weight: 5)
            });

            var onPage = score.Categories.Single(it => it.Category == SeoCheckCategory.OnPage);

            Assert.Multiple(() =>
            {
                Assert.That(onPage.Checks, Has.Count.EqualTo(2));
                Assert.That(onPage.Checks[0].Alias, Is.EqualTo("Broken"), "worst first");
                Assert.That(onPage.Checks[0].FailureRate, Is.EqualTo(0.8).Within(0.001));
                Assert.That(onPage.Checks[1].Score, Is.EqualTo(1).Within(0.001));
            });
        }

        [Test]
        public void TheVersionIsReportedSoStoredScoresStayComparable()
        {
            var score = Calculator.Calculate(new[] { Check("A") });

            Assert.That(score.Version, Is.EqualTo(HealthScoreCalculator.CurrentVersion));
        }

        [Test]
        public void CategoryWeightsCanBeOverridden()
        {
            var weights = new Dictionary<SeoCheckCategory, int>
            {
                [SeoCheckCategory.Images] = 100,
                [SeoCheckCategory.Indexability] = 1
            };

            var score = new HealthScoreCalculator(weights).Calculate(new[]
            {
                Check("Bad", SeoCheckCategory.Images, failed: 100),
                Check("Fine", SeoCheckCategory.Indexability, failed: 0)
            });

            //With images weighted far above indexability, the failure now dominates.
            Assert.That(score.Score, Is.LessThan(50));
        }
    }
}
