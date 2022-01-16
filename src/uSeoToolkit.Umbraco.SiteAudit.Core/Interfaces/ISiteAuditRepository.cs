using uSeoToolkit.Umbraco.Common.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteAuditRepository : IRepository<SiteAuditDto>
    {
        void SaveCrawledPage(SiteAuditDto audit, CrawledPageDto page);
    }
}
