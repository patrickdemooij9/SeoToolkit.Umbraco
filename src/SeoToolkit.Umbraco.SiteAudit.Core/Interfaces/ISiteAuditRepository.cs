using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteAuditRepository : IRepository<SiteAuditDto>
    {
        void SaveCrawledPage(SiteAuditDto audit, CrawledPageDto page);
    }
}
