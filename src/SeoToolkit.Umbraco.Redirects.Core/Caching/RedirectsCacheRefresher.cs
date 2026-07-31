using SeoToolkit.Umbraco.Redirects.Core.Constants;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    public sealed class RedirectsCacheRefresher : CacheRefresherBase<RedirectsCacheRefresherNotification>
    {
        public static Guid CacheGuid = new("3c46284f-3fad-49fe-9cad-f436f0762c5d");
        private readonly IRedirectsBloomFilter _bloomFilter;
        private readonly IRedirectsRepository _redirectsRepository;

        public override Guid RefresherUniqueId => CacheGuid;
        public override string Name => "Redirects Cache Refresher";

        public RedirectsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory, IRedirectsBloomFilter bloomFilter = null, IRedirectsRepository redirectsRepository = null) : base(appCaches, eventAggregator, factory)
        {
            _bloomFilter = bloomFilter;
            _redirectsRepository = redirectsRepository;
        }

        public override void Refresh(Guid key)
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.Redirects);

            // When the module is disabled these aren't registered, so there is nothing to keep in sync.
            if (_redirectsRepository is not null && _bloomFilter is not null)
            {
                var redirect = _redirectsRepository.Get(key);
                if (redirect is null)
                {
                    _bloomFilter.Remove(string.Empty); // Forces rebuild
                }
                else
                {
                    _bloomFilter.Add(redirect.OldUrl);
                }
            }

            base.Refresh(key);
        }
    }
}
