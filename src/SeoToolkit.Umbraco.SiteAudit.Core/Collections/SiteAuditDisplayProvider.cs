using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using System;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    [Weight(200)]
    public class SiteAuditDisplayProvider : ISeoDisplayProvider
    {
        public SeoDisplayViewModel Get(IContent content)
        {
            return new SeoDisplayViewModel()
            {
                Alias = "pageChecks",
                Name = "Page checks",
                View = "/App_Plugins/SeoToolkit/SiteAudit/Interface/SeoDisplays/pageChecks.html"
            };
        }
    }
}
