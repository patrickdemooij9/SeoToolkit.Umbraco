using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService
{
    public class SeoKeyValueService : ISeoKeyValueService
    {
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;
        private readonly IEventAggregator _eventAggregator;

        public SeoKeyValueService(ISeoDomainResolver seoDomainResolver, ISeoKeyValueRepository seoKeyValueRepository, IEventAggregator eventAggregator)
        {
            _seoDomainResolver = seoDomainResolver;
            _seoKeyValueRepository = seoKeyValueRepository;
            _eventAggregator = eventAggregator;
        }

        public string GetValue(string key)
        {
            var currentDomain = _seoDomainResolver.ResolveDomain();
            var rootValues = _seoKeyValueRepository.Get(null);
            if (currentDomain is null)
            {
                return rootValues.TryGetValue(key, out var value) ? value : null;
            }
            var domainValues = _seoKeyValueRepository.Get(currentDomain.Id.Value);
            if (domainValues.TryGetValue(key, out var domainValue))
            {
                return domainValue;
            }
            return rootValues.TryGetValue(key, out var rootValue) ? rootValue : null;
        }

        public void SaveValues(IDictionary<string, string?> values, Guid? domainId)
        {
            foreach (var (key, value) in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _seoKeyValueRepository.Delete(key, domainId);
                    continue;
                }

                _seoKeyValueRepository.Set(key, value, domainId);
            }

            _eventAggregator.Publish(new SeoKeyValueSavedNotification(domainId));
        }
    }
}
