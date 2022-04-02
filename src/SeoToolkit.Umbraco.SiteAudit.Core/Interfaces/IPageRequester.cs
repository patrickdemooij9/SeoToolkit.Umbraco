using System;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface IPageRequester
    {
        Task<CrawledPageModel> MakeRequest(Uri uri);
    }
}
