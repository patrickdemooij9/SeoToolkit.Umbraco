using System;
using Umbraco.Cms.Core.Dashboards;
using SeoToolkit.Umbraco.Common.Core.Sections;

namespace SeoToolkit.Umbraco.Common.Core.Dashboards
{
    public class WelcomeDashboard : IDashboard
    {
        public string Alias => "siteAuditWelcomeDashboard";

        public string View => "/App_Plugins/SeoToolkit/Dashboards/welcomeDashboard.html";
        public string[] Sections => new[] { SeoToolkitSection.SectionAlias };
        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}
