using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class SiteCheckDto
    {
        public int Id { get; set; }
        public ISiteCheck Check { get; set; }
    }
}
