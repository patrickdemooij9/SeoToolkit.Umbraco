#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Scoring
{
    public sealed class CheckScore
    {
        public required string Alias { get; init; }
        public required string Name { get; init; }
        public required SeoCheckCategory Category { get; init; }
        public required int Weight { get; init; }

        /// <summary>Share of applicable resources this check found a problem with, 0 to 1.</summary>
        public required double FailureRate { get; init; }

        /// <summary>0 to 1, where 1 is a clean pass.</summary>
        public required double Score { get; init; }
    }

    public sealed class CategoryScore
    {
        public required SeoCheckCategory Category { get; init; }
        public required int Weight { get; init; }

        /// <summary>0 to 100.</summary>
        public required double Score { get; init; }

        public required IReadOnlyList<CheckScore> Checks { get; init; }
    }

    public sealed class HealthScore
    {
        /// <summary>0 to 100.</summary>
        public required int Score { get; init; }

        public required int Version { get; init; }

        public required IReadOnlyList<CategoryScore> Categories { get; init; }

        /// <summary>False when nothing measurable ran, in which case there is no score to show.</summary>
        public required bool HasScore { get; init; }
    }

    /// <summary>
    /// Turns a run's per-check totals into a single number.
    /// <para>
    /// Screaming Frog has no score at all, so this is a genuine differentiator - but only if the
    /// number can be trusted. Four properties make it so, and each is pinned by a test:
    /// </para>
    /// <list type="bullet">
    /// <item>It is built from ratios, so crawling ten times more pages does not move it.</item>
    /// <item>A check that did not run leaves the denominator entirely, so enabling or disabling
    /// checks neither inflates nor deflates the result.</item>
    /// <item>Categories are weighted separately, so one noisy area cannot dominate.</item>
    /// <item>The version is stored with the score, so changing this formula later cannot quietly
    /// corrupt a historical trend.</item>
    /// </list>
    /// </summary>
    public class HealthScoreCalculator
    {
        /// <summary>
        /// Bumped whenever the formula changes. Stored per run so old scores stay comparable
        /// only with others of the same version.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// How much of a check's failure rate is subtracted. A page that cannot be indexed at all
        /// costs its full share; a notice barely registers.
        /// </summary>
        private static readonly IReadOnlyDictionary<SeoSeverity, double> SeverityPenalty =
            new Dictionary<SeoSeverity, double>
            {
                [SeoSeverity.Critical] = 1.0,
                [SeoSeverity.Error] = 0.6,
                [SeoSeverity.Warning] = 0.25,
                [SeoSeverity.Notice] = 0.05,
                [SeoSeverity.Passed] = 0.0
            };

        /// <summary>
        /// Relative importance of each area. Indexability leads because a page search engines
        /// cannot index is worth nothing however well written it is.
        /// </summary>
        private static readonly IReadOnlyDictionary<SeoCheckCategory, int> DefaultCategoryWeights =
            new Dictionary<SeoCheckCategory, int>
            {
                [SeoCheckCategory.Indexability] = 25,
                [SeoCheckCategory.Technical] = 20,
                [SeoCheckCategory.OnPage] = 20,
                [SeoCheckCategory.Content] = 15,
                [SeoCheckCategory.Images] = 10,
                [SeoCheckCategory.Links] = 10,
                [SeoCheckCategory.Performance] = 10,
                [SeoCheckCategory.Umbraco] = 10,
                [SeoCheckCategory.StructuredData] = 8,
                [SeoCheckCategory.International] = 8,
                [SeoCheckCategory.Accessibility] = 8,
                [SeoCheckCategory.Other] = 5
            };

        private readonly IReadOnlyDictionary<SeoCheckCategory, int> _categoryWeights;

        public HealthScoreCalculator(IReadOnlyDictionary<SeoCheckCategory, int>? categoryWeights = null)
        {
            _categoryWeights = categoryWeights ?? DefaultCategoryWeights;
        }

        public HealthScore Calculate(IReadOnlyList<AuditCheckRun> checkRuns)
        {
            if (checkRuns is null) throw new ArgumentNullException(nameof(checkRuns));

            var scored = checkRuns
                // A check that was skipped, or that nothing on the site was applicable to, tells
                // us nothing - so it leaves the calculation rather than counting as a pass.
                .Where(it => it.DidRun && it.ApplicableCount > 0 && it.Weight > 0)
                .Select(ToCheckScore)
                .ToArray();

            if (scored.Length == 0)
            {
                return new HealthScore
                {
                    Score = 0,
                    Version = CurrentVersion,
                    Categories = Array.Empty<CategoryScore>(),
                    HasScore = false
                };
            }

            var categories = scored
                .GroupBy(it => it.Category)
                .Select(BuildCategory)
                .OrderBy(it => it.Category.ToString(), StringComparer.Ordinal)
                .ToArray();

            var totalWeight = categories.Sum(it => it.Weight);
            var overall = totalWeight == 0
                ? 0
                : categories.Sum(it => it.Weight * it.Score) / totalWeight;

            return new HealthScore
            {
                // Rounded once, at the end. Rounding per category would let the parts disagree
                // with the whole in a way that is hard to explain to whoever is reading it.
                Score = (int)Math.Round(Clamp(overall), MidpointRounding.AwayFromZero),
                Version = CurrentVersion,
                Categories = categories,
                HasScore = true
            };
        }

        private CheckScore ToCheckScore(AuditCheckRun checkRun)
        {
            // Against applicable resources, not the whole crawl: a check that only looks at
            // images should not be diluted by every page that is not one.
            var failureRate = Clamp01((double)checkRun.FailedCount / checkRun.ApplicableCount);
            var penalty = SeverityPenalty.TryGetValue(checkRun.Severity, out var value) ? value : 0.25;

            return new CheckScore
            {
                Alias = checkRun.CheckAlias,
                Name = checkRun.CheckName ?? checkRun.CheckAlias,
                Category = checkRun.Category,
                Weight = checkRun.Weight,
                FailureRate = failureRate,
                Score = Clamp01(1 - failureRate * penalty)
            };
        }

        private CategoryScore BuildCategory(IGrouping<SeoCheckCategory, CheckScore> group)
        {
            var checks = group.OrderBy(it => it.Score).ThenBy(it => it.Name, StringComparer.Ordinal).ToArray();
            var weightSum = checks.Sum(it => it.Weight);

            var score = weightSum == 0
                ? 100
                : 100 * checks.Sum(it => it.Weight * it.Score) / weightSum;

            return new CategoryScore
            {
                Category = group.Key,
                Weight = _categoryWeights.TryGetValue(group.Key, out var weight) ? weight : 5,
                Score = Clamp(score),
                Checks = checks
            };
        }

        private static double Clamp(double value) => value < 0 ? 0 : value > 100 ? 100 : value;

        private static double Clamp01(double value) => value < 0 ? 0 : value > 1 ? 1 : value;
    }
}
