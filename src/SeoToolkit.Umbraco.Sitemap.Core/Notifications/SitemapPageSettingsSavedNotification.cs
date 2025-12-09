using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.Sitemap.Core.Notifications
{
    public class SitemapPageSettingsSavedNotification : INotification
    {
        public SitemapPageSettings Model { get; }

        public SitemapPageSettingsSavedNotification(SitemapPageSettings model)
        {
            Model = model;
        }
    }
}
