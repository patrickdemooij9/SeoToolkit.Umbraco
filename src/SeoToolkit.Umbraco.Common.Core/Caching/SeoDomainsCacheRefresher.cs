using SeoToolkit.Umbraco.Common.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Common.Core.Caching
{
    public sealed class SeoDomainsCacheRefresher : CacheRefresherBase<SeoDomainsCacheRefresherNotification>
    {
        public static Guid CacheRefreshGuid => new("6abe3275-0c29-4e95-920b-18157c60376f");

        public SeoDomainsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override Guid RefresherUniqueId => CacheRefreshGuid;

        public override string Name => "Seo Domains Cache Refresher";

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey($"{CacheConstants.SeoDomains}");
            base.RefreshAll();
        }
    }
}
