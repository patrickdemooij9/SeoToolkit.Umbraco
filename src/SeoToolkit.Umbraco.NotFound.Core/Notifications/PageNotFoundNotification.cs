using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.NotFound.Core.Notifications
{
    public class PageNotFoundNotification : INotification
    {
        public IPublishedContent Page { get; set; }

        public PageNotFoundNotification(IPublishedContent page)
        {
            Page = page;
        }
    }
}