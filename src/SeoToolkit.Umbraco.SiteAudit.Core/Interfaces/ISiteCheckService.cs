using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCheckService
    {
        IEnumerable<SiteCheckDto> GetAll();
        SiteCheckDto GetCheckById(int id);
    }
}
