using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class SiteCheckDto
    {
        public int Id { get; set; }
        public ISiteCheck Check { get; set; }
    }
}
