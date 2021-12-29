using Umbraco.Cms.Core.Dashboards;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Dashboards
{
    public class SiteAuditDashboard : IDashboard
    {
        public string[] Sections => new string[] { "uSeoToolkitSection" };

        public IAccessRule[] AccessRules => new IAccessRule[0];

        public string Alias => "siteAuditDashboard";

        public string View => "/App_Plugins/uSeoToolkitSiteAudit/backoffice/SiteAudit/list.html";
    }
}
