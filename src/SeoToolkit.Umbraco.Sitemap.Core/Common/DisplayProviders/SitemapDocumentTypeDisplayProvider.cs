using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.DisplayProviders
{
    [Weight(200)]
    public class SitemapDocumentTypeDisplayProvider : IDisplayProvider
    {
        public SeoSettingsDisplayViewModel Get(int contentTypeId)
        {
            return new SeoSettingsDisplayViewModel
            {
                Alias = "sitemap",
                Name = "Sitemap",
                View = "/App_Plugins/SeoToolkit/Sitemap/Displays/DocumentType/sitemapSettings.html"
            };
        }
    }
}
