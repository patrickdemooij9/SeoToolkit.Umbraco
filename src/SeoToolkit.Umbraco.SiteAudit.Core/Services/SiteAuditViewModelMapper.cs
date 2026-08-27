#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    /// <summary>
    /// Turns stored audit data into what the backoffice reads.
    /// <para>
    /// Kept out of the controllers, which the previous version did not do - it built view models
    /// inline and left several TODOs saying as much.
    /// </para>
    /// </summary>
    public class SiteAuditViewModelMapper
    {
        private readonly ISeoCheckCatalogue _catalogue;

        public SiteAuditViewModelMapper(ISeoCheckCatalogue catalogue)
        {
            _catalogue = catalogue;
        }

        public SiteAuditRunOverviewViewModel MapOverview(AuditRun run) => new()
        {
            Id = run.Id,
            Key = run.Key,
            Name = run.Name,
            Status = run.Status.ToString(),
            CreatedDate = run.CreatedUtc.ToLocalTime().ToString("g", CultureInfo.InvariantCulture),
            TotalCrawled = run.TotalCrawled,
            ErrorCount = run.ErrorCount + run.CriticalCount,
            WarningCount = run.WarningCount,
            Score = run.Score
        };

        public SiteAuditRunStatusViewModel MapStatus(AuditRun run) => Fill(new SiteAuditRunStatusViewModel(), run);

        public SiteAuditRunDetailViewModel MapDetail(AuditRun run)
        {
            var model = Fill(new SiteAuditRunDetailViewModel(), run);

            model.Key = run.Key;
            model.Name = run.Name;
            model.StartingUrl = run.StartingUrl.ToString();
            model.BaseUrl = run.BaseUrl;
            model.CreatedDate = run.CreatedUtc.ToLocalTime().ToString("g", CultureInfo.InvariantCulture);
            model.StartedDate = run.StartedUtc?.ToLocalTime().ToString("g", CultureInfo.InvariantCulture);
            model.FinishedDate = run.FinishedUtc?.ToLocalTime().ToString("g", CultureInfo.InvariantCulture);
            model.MaxPages = run.MaxPages;

            var config = AuditRunConfig.FromJson(run.ConfigJson);
            model.StartNodeKey = config.StartNodeKey;
            model.Culture = config.Culture;

            return model;
        }

        public SiteAuditStartNodeViewModel MapStartNode(AuditStartNode node) => new()
        {
            Key = node.Key,
            Name = node.Name,
            VariesByCulture = node.VariesByCulture,
            Url = node.Url,
            Cultures = node.Cultures.Select(it => new SiteAuditStartNodeCultureViewModel
            {
                IsoCode = it.IsoCode,
                Name = it.Name,
                Url = it.Url
            }).ToArray()
        };

        private static T Fill<T>(T model, AuditRun run) where T : SiteAuditRunStatusViewModel
        {
            model.Id = run.Id;
            model.Status = run.Status.ToString();
            model.IsFinished = run.IsFinished;
            model.Progress = Math.Round(run.Progress, 1);
            model.TotalCrawled = run.TotalCrawled;
            model.TotalDiscovered = run.TotalDiscovered;
            model.CriticalCount = run.CriticalCount;
            model.ErrorCount = run.ErrorCount;
            model.WarningCount = run.WarningCount;
            model.NoticeCount = run.NoticeCount;
            model.Score = run.Score;

            return model;
        }

        public SiteAuditResourceViewModel MapResource(AuditResource resource) => Fill(new SiteAuditResourceViewModel(), resource);

        public SiteAuditResourceDetailViewModel MapResourceDetail(AuditResource resource, IReadOnlyList<AuditIssue> issues)
        {
            var model = Fill(new SiteAuditResourceDetailViewModel(), resource);

            model.FinalUrl = resource.FinalUrl?.ToString();
            model.ContentType = resource.ContentType;
            model.Culture = resource.Culture;
            model.Issues = issues.Select(MapIssue).ToArray();

            return model;
        }

        private static T Fill<T>(T model, AuditResource resource) where T : SiteAuditResourceViewModel
        {
            model.Id = resource.Id;
            model.Url = resource.Url.ToString();
            model.Path = resource.Url.PathAndQuery;
            model.StatusCode = resource.StatusCode;
            model.Kind = resource.Kind.ToString();
            model.Depth = resource.Depth;
            model.IsInternal = resource.IsInternal;
            model.ResponseTimeMs = resource.ResponseTimeMs;
            model.SizeBytes = resource.SizeBytes;
            model.RedirectCount = resource.RedirectCount;
            model.Title = resource.Title;
            model.MetaDescription = resource.MetaDescription;
            model.H1 = resource.H1;
            model.WordCount = resource.WordCount;
            model.IsIndexable = (resource.Indexability & IndexabilityFlags.Indexable) != 0;
            model.IndexabilityReasons = DescribeIndexability(resource.Indexability);
            model.Failure = resource.Failure?.ToString();
            model.UmbracoContentKey = resource.UmbracoContentKey;
            model.IssueCount = resource.IssueCount;
            model.ErrorCount = resource.ErrorCount;
            model.WarningCount = resource.WarningCount;

            return model;
        }

        /// <summary>
        /// Lists the reasons a page is not indexable. Editors need to know which of several
        /// possible causes applies, not just that something is wrong.
        /// </summary>
        private static IReadOnlyList<string> DescribeIndexability(IndexabilityFlags flags)
        {
            if ((flags & IndexabilityFlags.Indexable) != 0) return Array.Empty<string>();

            var reasons = new List<string>(2);

            if (flags.HasFlag(IndexabilityFlags.NoIndex)) reasons.Add("NoIndex");
            if (flags.HasFlag(IndexabilityFlags.BlockedByRobotsTxt)) reasons.Add("BlockedByRobotsTxt");
            if (flags.HasFlag(IndexabilityFlags.CanonicalisedAway)) reasons.Add("CanonicalisedAway");
            if (flags.HasFlag(IndexabilityFlags.Redirected)) reasons.Add("Redirected");
            if (flags.HasFlag(IndexabilityFlags.ErrorStatus)) reasons.Add("ErrorStatus");
            if (flags.HasFlag(IndexabilityFlags.Unreachable)) reasons.Add("Unreachable");
            if (flags.HasFlag(IndexabilityFlags.External)) reasons.Add("External");

            return reasons;
        }

        public SiteAuditIssueViewModel MapIssue(AuditIssue issue)
        {
            var descriptor = _catalogue.Get(issue.CheckAlias);

            return new SiteAuditIssueViewModel
            {
                Id = issue.Id,
                CheckAlias = issue.CheckAlias,
                CheckName = descriptor?.Name ?? issue.CheckAlias,
                Category = (descriptor?.Category ?? SeoCheckCategory.Other).ToString(),
                Variant = issue.Variant,
                Severity = issue.Severity.ToString(),
                IsError = issue.Severity >= SeoSeverity.Error,
                IsWarning = issue.Severity == SeoSeverity.Warning,
                Message = _catalogue.Describe(issue),
                Url = issue.Url?.ToString(),
                Evidence = issue.Evidence,
                ResourceId = issue.ResourceId,
                Data = issue.Data
            };
        }

        /// <summary>
        /// Groups per-check totals by category, which is how the summary is read and how the
        /// health score will weight them.
        /// </summary>
        public SiteAuditSummaryViewModel MapSummary(IReadOnlyList<AuditCheckRun> checkRuns)
        {
            var categories = checkRuns
                .GroupBy(it => it.Category)
                .Select(group => new SiteAuditCategorySummaryViewModel
                {
                    Category = group.Key.ToString(),
                    FailedCount = group.Sum(it => it.FailedCount),
                    IssueCount = group.Sum(it => it.IssueCount),
                    Checks = group
                        .OrderByDescending(it => it.FailedCount)
                        .ThenBy(it => it.CheckName)
                        .Select(MapCheckSummary)
                        .ToArray()
                })
                // Categories with real findings first: the summary is read top down.
                .OrderByDescending(it => it.IssueCount)
                .ThenBy(it => it.Category)
                .ToArray();

            return new SiteAuditSummaryViewModel { Categories = categories };
        }

        private SiteAuditCheckSummaryViewModel MapCheckSummary(AuditCheckRun checkRun)
        {
            var descriptor = _catalogue.Get(checkRun.CheckAlias);

            return new SiteAuditCheckSummaryViewModel
            {
                Alias = checkRun.CheckAlias,
                Name = checkRun.CheckName ?? descriptor?.Name ?? checkRun.CheckAlias,
                Description = descriptor?.Description,
                Category = checkRun.Category.ToString(),
                Severity = checkRun.Severity.ToString(),
                DidRun = checkRun.DidRun,
                ApplicableCount = checkRun.ApplicableCount,
                FailedCount = checkRun.FailedCount,
                IssueCount = checkRun.IssueCount,
                Weight = checkRun.Weight
            };
        }

        public SiteAuditCheckCatalogueViewModel MapCatalogueEntry(SeoCheckDescriptor descriptor) => new()
        {
            Alias = descriptor.Alias,
            Name = descriptor.Name,
            Description = descriptor.Description,
            Category = descriptor.Category.ToString(),
            DefaultSeverity = descriptor.DefaultSeverity.ToString(),
            Weight = descriptor.Weight,
            SupportsSinglePage = descriptor.SupportsSinglePage,
            DocumentationUrl = descriptor.DocumentationUrl,
            ProviderName = descriptor.ProviderName,
            RequiresFeature = descriptor.RequiresFeature,
            IsAvailable = _catalogue.IsAvailable(descriptor),
            Options = descriptor.Options.Values.Select(option => new SiteAuditCheckOptionViewModel
            {
                Key = option.Key,
                Name = option.Name,
                Description = option.Description,
                Type = option.Type.ToString(),
                DefaultValue = option.DefaultValue,
                Minimum = option.Minimum,
                Maximum = option.Maximum
            }).ToArray()
        };
    }
}
