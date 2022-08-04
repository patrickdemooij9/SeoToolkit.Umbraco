using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Notifications
{
    public class AfterMetaTagsNotification : INotification
    {
        public string ContentTypeAlias { get; }
        public MetaTagsModel MetaTags { get; }

        public AfterMetaTagsNotification(string contentTypeAlias, MetaTagsModel metaTags)
        {
            ContentTypeAlias = contentTypeAlias;
            MetaTags = metaTags;
        }
    }
}
