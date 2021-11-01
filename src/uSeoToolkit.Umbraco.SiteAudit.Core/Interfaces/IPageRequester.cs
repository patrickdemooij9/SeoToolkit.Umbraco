using System;
using System.Threading.Tasks;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface IPageRequester
    {
        Task<CrawledPageModel> MakeRequest(Uri uri);
    }
}
