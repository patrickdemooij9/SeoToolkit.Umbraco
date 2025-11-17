using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Services
{
    public class PageNotFoundService : IPageNotFoundService
    {
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;

        public PageNotFoundService(ISeoKeyValueRepository seoKeyValueRepository)
        {
            _seoKeyValueRepository = seoKeyValueRepository;
        }

        public Guid? GetPageNotFound(Guid? domainId)
        {
            var value = _seoKeyValueRepository.Get(NotFoundConstants.NotFoundKeyValueKey, domainId);
            if (Guid.TryParse(value, out var pageNotFound))
            {
                return pageNotFound;
            }
            return null;
        }

        public void SetPageNotFound(Guid? nodeId, Guid? domainId)
        {
            if (nodeId.HasValue)
            {
                _seoKeyValueRepository.Set(NotFoundConstants.NotFoundKeyValueKey, nodeId.Value.ToString(), domainId);
            }
            else
            {
                _seoKeyValueRepository.Delete(NotFoundConstants.NotFoundKeyValueKey, domainId);
            }
        }
    }
}
