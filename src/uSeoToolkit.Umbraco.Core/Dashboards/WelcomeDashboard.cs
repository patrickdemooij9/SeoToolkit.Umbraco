using System;
using Umbraco.Cms.Core.Dashboards;
using uSeoToolkit.Umbraco.Common.Core.Sections;

namespace uSeoToolkit.Umbraco.Common.Core.Dashboards
{
    public class WelcomeDashboard : IDashboard
    {
        public string Alias => "siteAuditWelcomeDashboard";

        public string View => "/App_Plugins/uSeoToolkit/Dashboards/welcomeDashboard.html";
        public string[] Sections => new[] { USeoToolkitSection.SectionAlias };
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
