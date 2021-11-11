using System.Linq;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels
{
    public class SiteAuditDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? MaxPagesToCrawl { get; set; }
        public int TotalPagesFound { get; set; }
        public float Progress { get; set; }
        public string Status { get; set; }
        public SiteAuditCheckViewModel[] Checks { get; set; }
        public SiteAuditPageDetailViewModel[] PagesCrawled { get; set; }

        public SiteAuditDetailViewModel(SiteAuditDto model)
        {
            //TODO: Move most of this to a mapper

            Id = model.Id;
            Name = model.Name;
            MaxPagesToCrawl = model.MaxPagesToCrawl;
            TotalPagesFound = model.TotalPagesFound;
            Status = model.Status.ToString();
            Checks = model.SiteChecks.Select(it => new SiteAuditCheckViewModel
            {
                Id = it.Id,
                Name = it.Check.Name,
                Description = it.Check.Description,
                ErrorMessage = it.Check.ErrorMessage
            }).ToArray();
            PagesCrawled = model.CrawledPages.Select(it => new SiteAuditPageDetailViewModel
            {
                Url = it.PageUrl.PathAndQuery,
                StatusCode = it.StatusCode,
                Results = it.Results.Select(r => new SiteAuditResultDetailViewModel
                {
                    CheckId = r.Check.Id,
                    Message = r.Check.Check.FormatMessage(new CheckPageCrawlResult { Result = r.Result, ExtraValues = r.ExtraValues }),
                    IsError = r.Result == SiteCrawlResultType.Error,
                    IsWarning = r.Result == SiteCrawlResultType.Warning
                }).ToArray()
            }).ToArray();

            if (model.Status == SiteAuditStatus.Running)
            {
                if (TotalPagesFound == 0)
                    Progress = 0;
                else if (MaxPagesToCrawl is null)
                {
                    Progress = (float)PagesCrawled.Length / TotalPagesFound * 100;
                }
                else
                {
                    Progress = (float)PagesCrawled.Length / MaxPagesToCrawl.Value * 100;
                }
            }
            else
            {
                Progress = 100;
            }
        }
    }
}
