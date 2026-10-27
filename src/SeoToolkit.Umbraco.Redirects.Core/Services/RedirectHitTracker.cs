using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;

namespace SeoToolkit.Umbraco.Redirects.Core.Services
{
    public class RedirectHitTracker : IRedirectHitTracker
    {
        private readonly IRedirectsRepository _redirectsRepository;
        private readonly ConcurrentDictionary<Guid, DateTime> _lastHits = new();

        public RedirectHitTracker(IRedirectsRepository redirectsRepository)
        {
            _redirectsRepository = redirectsRepository;
        }

        public void RegisterHit(Guid redirectKey)
        {
            _lastHits[redirectKey] = DateTime.UtcNow;
        }

        public void Flush()
        {
            var lastHits = new Dictionary<Guid, DateTime>();
            foreach (var key in _lastHits.Keys)
            {
                if (_lastHits.TryRemove(key, out var lastHit))
                {
                    lastHits[key] = lastHit;
                }
            }

            if (lastHits.Count == 0)
                return;

            _redirectsRepository.UpdateLastHits(lastHits);
        }
    }
}
