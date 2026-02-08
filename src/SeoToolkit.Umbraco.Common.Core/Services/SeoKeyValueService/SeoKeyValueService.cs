using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService
{
    public class SeoKeyValueService : ISeoKeyValueService
    {
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;

        public SeoKeyValueService(ISeoDomainResolver seoDomainResolver, ISeoKeyValueRepository seoKeyValueRepository)
        {
            _seoDomainResolver = seoDomainResolver;
            _seoKeyValueRepository = seoKeyValueRepository;
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
    }
}
