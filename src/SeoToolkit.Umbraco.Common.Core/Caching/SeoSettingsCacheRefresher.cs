using SeoToolkit.Umbraco.Common.Core.Constants;
using System;
using System.Security.Policy;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Common.Core.Caching
{
    public sealed class SeoSettingsCacheRefresher : CacheRefresherBase<SeoSettingsCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("835839b0-457a-4a02-b5c8-3f8cfec5e350");

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "Seo Settings Cache Refresher";

        public SeoSettingsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override void Refresh(int id)
        {
            AppCaches.RuntimeCache.ClearByKey($"{CacheConstants.SeoSettings}{id}");
            base.Refresh(id);
        }
    }
}
