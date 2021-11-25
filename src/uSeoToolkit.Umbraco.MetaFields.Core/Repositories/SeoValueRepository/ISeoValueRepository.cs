using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public interface ISeoValueRepository
    {
        void Add(int nodeId, string fieldAlias, object value);
        void Update(int nodeId, string fieldAlias, object value);
        void Delete(int nodeId, string fieldAlias);
        bool Exists(int nodeId, string fieldAlias);

        Dictionary<string, object> GetAllValues(int nodeId);
    }
}
