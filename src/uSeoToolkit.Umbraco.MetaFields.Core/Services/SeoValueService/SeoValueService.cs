using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService
{
    public class SeoValueService : ISeoValueService
    {
        private readonly ISeoValueRepository _repository;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public SeoValueService(ISeoValueRepository repository, IVariationContextAccessor variationContextAccessor)
        {
            _repository = repository;
            _variationContextAccessor = variationContextAccessor;
        }

        public Dictionary<string, object> GetUserValues(int nodeId)
        {
            return _repository.GetAllValues(nodeId, GetCulture());
        }

        public void AddValues(int nodeId, Dictionary<string, object> values)
        {
            foreach (var (key, value) in values)
            {
                if (_repository.Exists(nodeId, key, GetCulture()))
                    _repository.Update(nodeId, key, GetCulture(), value);
                else
                    _repository.Add(nodeId, key, GetCulture(), value);
            }
        }

        public void Delete(int nodeId, string fieldAlias)
        {
            _repository.Delete(nodeId, fieldAlias, GetCulture());
        }

        private string GetCulture()
        {
            return _variationContextAccessor.VariationContext.Culture;
        }
    }
}
