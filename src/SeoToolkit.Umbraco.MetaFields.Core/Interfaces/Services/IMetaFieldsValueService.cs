using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services
{
    public interface IMetaFieldsValueService
    {
        /// <summary>
        /// Get the values set by the user. If culture is NULL, the variation context culture will be used.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        [Obsolete("Use the overload with Guid key instead")]
        Dictionary<string, object> GetUserValues(int nodeId, string culture = null);

        /// <summary>
        /// Set the values by the user. If culture is NULL, the variation context culture will be used.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="values"></param>
        /// <param name="culture"></param>
        [Obsolete("Use the overload with Guid key instead")]
        void AddValues(int nodeId, Dictionary<string, object> values, string culture = null);
        [Obsolete("Use the overload with Guid key instead")]
        void Delete(int nodeId, string fieldAlias, string culture = null);

        /// <summary>
        /// Get the values set by the user. If culture is NULL, the variation context culture will be used.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        Dictionary<string, object> GetUserValues(Guid nodeId, string culture = null);

        /// <summary>
        /// Set the values by the user. If culture is NULL, the variation context culture will be used.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="values"></param>
        /// <param name="culture"></param>
        void AddValues(Guid nodeId, Dictionary<string, object> values, string culture = null);
        void Delete(Guid nodeId, string fieldAlias, string culture = null);
    }
}
