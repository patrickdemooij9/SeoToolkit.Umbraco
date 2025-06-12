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

        public RedirectsCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory, IRedirectsBloomFilter bloomFilter, IRedirectsRepository redirectsRepository) : base(appCaches, eventAggregator, factory)
        {
            _bloomFilter = bloomFilter;
            _redirectsRepository = redirectsRepository;
        }

        public override void Refresh(int id)
        {
            AppCaches.RuntimeCache.ClearByKey(CacheConstants.Redirects);
            var redirect = _redirectsRepository.Get(id);
            if (redirect is null)
            {
                _bloomFilter.Remove(string.Empty); // Forces rebuild
            }
            else
            {
                _bloomFilter.Add(redirect.OldUrl);
            }
            
            base.Refresh(id);
        }
    }
}
