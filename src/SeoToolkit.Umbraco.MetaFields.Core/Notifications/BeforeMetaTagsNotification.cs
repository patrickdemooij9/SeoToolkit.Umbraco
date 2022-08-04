using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Notifications
{
    public class BeforeMetaTagsNotification : INotification
    {
        public string ContentTypeAlias { get; }
        public MetaTagsModel MetaTags { get; }

        public BeforeMetaTagsNotification(string contentTypeAlias, MetaTagsModel metaTags)
        {
            ContentTypeAlias = contentTypeAlias;
            MetaTags = metaTags;
        }
    }
}
