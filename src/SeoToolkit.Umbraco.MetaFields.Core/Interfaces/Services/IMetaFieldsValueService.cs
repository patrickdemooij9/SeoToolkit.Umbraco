using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services
{
    public interface IMetaFieldsValueService
    {
        Dictionary<string, object> GetUserValues(int nodeId, string culture = null);
        void AddValues(int nodeId, Dictionary<string, object> values, string culture = null);
        void Delete(int nodeId, string fieldAlias);
    }
}
