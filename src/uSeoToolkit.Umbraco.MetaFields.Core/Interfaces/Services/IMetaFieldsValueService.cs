using System;
using System.Collections.Generic;
using System.Text;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services
{
    public interface IMetaFieldsValueService
    {
        Dictionary<string, object> GetUserValues(int nodeId);
        void AddValues(int nodeId, Dictionary<string, object> values);
        void Delete(int nodeId, string fieldAlias);
    }
}
