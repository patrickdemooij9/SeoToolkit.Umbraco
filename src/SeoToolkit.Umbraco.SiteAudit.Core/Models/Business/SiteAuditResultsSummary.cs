using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    /// <summary>
    /// The results of one audit per page, without the extra values of each result. Cheap enough for overviews.
    /// </summary>
    public class SiteAuditResultsSummary
    {
        public int AuditId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<SiteCheckDto> Checks { get; set; } = new();
        public List<SiteAuditPageResults> Pages { get; set; } = new();

        /// <summary>
        /// Pages with an error count as 0, pages with only warnings as half, clean pages as 1. Null without pages.
        /// </summary>
        public int? GetScore()
        {
            if (Pages.Count == 0)
                return null;

            var points = Pages.Sum(page =>
            {
                if (page.Results.Any(it => it.Result == SiteCrawlResultType.Error)) return 0m;
                if (page.Results.Any(it => it.Result == SiteCrawlResultType.Warning)) return 0.5m;
                return 1m;
            });
            return (int)Math.Round(points / Pages.Count * 100, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Pages with an error or warning for any of the given checks.
        /// </summary>
        public List<SiteAuditPageResults> GetPagesFailing(params int[] checkIds)
        {
            return Pages.Where(page => page.Results.Any(it => checkIds.Contains(it.CheckId) && it.IsFailure)).ToList();
        }
    }

    public class SiteAuditPageResults
    {
        public string Url { get; set; }
        public List<SiteAuditPageResult> Results { get; set; } = new();
    }

    public class SiteAuditPageResult
    {
        public int CheckId { get; set; }
        public SiteCrawlResultType Result { get; set; }

        public bool IsFailure => Result is SiteCrawlResultType.Error or SiteCrawlResultType.Warning;
    }
}
