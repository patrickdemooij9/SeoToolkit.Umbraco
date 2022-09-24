using Umbraco.Cms.Core.Dashboards;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Dashboards
{
    public class SiteAuditDashboard : IDashboard
    {
        public string[] Sections => new string[] { "SeoToolkitSection" };

        public IAccessRule[] AccessRules => new IAccessRule[0];

        public string Alias => "siteAuditDashboard";

        public string View => "/App_Plugins/SeoToolkit/backoffice/SiteAudit/list.html";
    }
}
