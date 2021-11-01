using System.Collections.Generic;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ISiteCheckService
    {
        IEnumerable<SiteCheckDto> GetAll();
        SiteCheckDto GetCheckById(int id);
    }
}
