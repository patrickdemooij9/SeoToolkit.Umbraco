using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public interface IMetaFieldsValueRepository
    {
        void Add(int nodeId, string fieldAlias, string culture, object value);
        void Update(int nodeId, string fieldAlias, string culture, object value);
        void Delete(int nodeId, string fieldAlias, string culture);
        bool Exists(int nodeId, string fieldAlias, string culture);

        Dictionary<string, object> GetAllValues(int nodeId, string culture);
    }
}
