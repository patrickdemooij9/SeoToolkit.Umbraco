using SeoToolkit.Umbraco.Common.Core.Constants;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Common.Core.Caching
{
    /// <summary>
    /// Clears the cached key/values on every server: one domain collection's by its id, or all of them, the root values included.
    /// </summary>
    public sealed class SeoKeyValuesCacheRefresher : CacheRefresherBase<SeoKeyValuesCacheRefresherNotification>
    {
        public static readonly Guid CacheGuid = new("4f0c8a3e-2b6d-4c1a-9e57-7d3b1f6a0c92");

        public SeoKeyValuesCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory) : base(appCaches, eventAggregator, factory)
        {
        }

        public override Guid RefresherUniqueId => CacheGuid;

        public override string Name => "Seo Key Values Cache Refresher";

        public static string CacheKey(Guid? domainId) => $"{CacheConstants.SeoKeyValues}{domainId ?? Guid.Empty}";

        public override void Refresh(Guid id)
        {
            AppCaches.RuntimeCache.ClearByKey(CacheKey(id));
            base.Refresh(id);
        }

        public override void RefreshAll()
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.SeoKeyValues);
            base.RefreshAll();
        }
    }
}
