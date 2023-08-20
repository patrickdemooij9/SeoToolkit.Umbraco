using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.MetaFields.Core.Caching
{
    public class SeoValueCacheRefresher : CacheRefresherBase<SeoValueCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("bf0f9b10-f56d-4385-9d48-0732c8e76846");

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "Seo Value Cache Refresher";

        public SeoValueCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void Refresh(int id)
        {
            AppCaches.RuntimeCache.ClearByKey($"{CacheConstants.SeoValue}{id}");
            base.Refresh(id);
        }
    }
}
