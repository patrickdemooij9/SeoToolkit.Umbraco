using System.Collections.Generic;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService
{
    public class SeoValueService : ISeoValueService
    {
        private readonly ISeoValueRepository _repository;

        public SeoValueService(ISeoValueRepository repository)
        {
            _repository = repository;
        }

        public Dictionary<string, object> GetUserValues(int nodeId)
        {
            return _repository.GetAllValues(nodeId);
        }

        public void AddValues(int nodeId, Dictionary<string, object> values)
        {
            foreach (var (key, value) in values)
            {
                if (_repository.Exists(nodeId, key))
                    _repository.Update(nodeId, key, value);
                else
                    _repository.Add(nodeId, key, value);
            }
        }

        public void Delete(int nodeId, string fieldAlias)
        {
            _repository.Delete(nodeId, fieldAlias);
        }
    }
}
