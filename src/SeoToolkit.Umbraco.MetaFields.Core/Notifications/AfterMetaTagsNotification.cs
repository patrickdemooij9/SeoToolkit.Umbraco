using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.MetaFields.Core.Notifications
{
    public class AfterMetaTagsNotification : INotification
    {
        public string ContentTypeAlias { get; }
        public MetaTagsModel MetaTags { get; }
        public IPublishedContent Content { get; }

        [Obsolete("This constructor is deprecated and will be removed in the next major release.")]
        public AfterMetaTagsNotification(string contentTypeAlias, MetaTagsModel metaTags)
        {
            ContentTypeAlias = contentTypeAlias;
            MetaTags = metaTags;
        }

        public AfterMetaTagsNotification(IPublishedContent content, MetaTagsModel metaTags)
        {
            Content = content;
            ContentTypeAlias = content.ContentType.Alias;
            MetaTags = metaTags;
        }
    }
}
