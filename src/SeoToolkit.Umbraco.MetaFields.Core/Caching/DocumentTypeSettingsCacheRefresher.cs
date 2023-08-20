using NPoco;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.MetaFields.Core.Caching
{
    public sealed class DocumentTypeSettingsCacheRefresher : CacheRefresherBase<DocumentTypeSettingsCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("6ffa6a98-95d3-4060-bcfb-63be3875da0c");

        public override Guid RefresherUniqueId => CacheGuid;

        public override string Name => "Document Type Settings Cache Refresher";

        public DocumentTypeSettingsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void Refresh(int id)
        {
            AppCaches.RuntimeCache.ClearByKey($"{CacheConstants.DocumentTypeSettings}{id}");
            base.Refresh(id);
        }
    }
}
