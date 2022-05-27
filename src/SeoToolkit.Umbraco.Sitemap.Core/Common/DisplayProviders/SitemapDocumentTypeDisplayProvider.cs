using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.Sitemap.Core.Common.DisplayProviders
{
    [Weight(200)]
    public class SitemapDocumentTypeDisplayProvider : IDisplayProvider
    {
        public SeoDisplayViewModel Get(int contentTypeId)
        {
            return new SeoDisplayViewModel
            {
                Alias = "sitemap",
                Name = "Sitemap",
                View = "/App_Plugins/SeoToolkit/Sitemap/Displays/DocumentType/sitemapSettings.html"
            };
        }
    }
}
