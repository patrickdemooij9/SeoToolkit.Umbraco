using System;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface IExternalSiteAuditClient
    {
        bool IsEnabled();
        Task<Guid?> StartAudit(ExternalSiteAuditCreateRequestDto request);
        Task<SiteAuditDetailViewModel> GetAuditDetail(Guid auditId);
        Task StopAudit(Guid auditId);
    }
}
