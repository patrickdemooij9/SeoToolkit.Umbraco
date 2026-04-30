using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository
{
    public interface IMetaFieldsValueRepository
    {
        [Obsolete("Use the overload with Guid key instead")]
        void Add(int nodeId, string fieldAlias, string culture, object value);
        [Obsolete("Use the overload with Guid key instead")]
        void Update(int nodeId, string fieldAlias, string culture, object value);
        [Obsolete("Use the overload with Guid key instead")]
        void Delete(int nodeId, string fieldAlias, string culture);
        [Obsolete("Use the overload with Guid key instead")]
        bool Exists(int nodeId, string fieldAlias, string culture);
        
        [Obsolete("Use the overload with Guid key instead")]
        Dictionary<string, object> GetAllValues(int nodeId, string culture);

        void Add(Guid nodeId, string fieldAlias, string culture, object value);
        void Update(Guid nodeId, string fieldAlias, string culture, object value);
        void Delete(Guid nodeId, string fieldAlias, string culture);
        bool Exists(Guid nodeId, string fieldAlias, string culture);

        Dictionary<string, object> GetAllValues(Guid nodeId, string culture);

        IEnumerable<(Guid NodeKey, string UserValue)> GetAllValuesByFieldAlias(string fieldAlias, string culture);
    }
}
