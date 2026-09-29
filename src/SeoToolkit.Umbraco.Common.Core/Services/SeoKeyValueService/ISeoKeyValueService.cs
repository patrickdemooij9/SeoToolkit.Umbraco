using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService
{
    public interface ISeoKeyValueService
    {
        public string? GetValue(string key);

        /// <summary>
        /// Saves the given key/values for the domain collection (or the root when <paramref name="domainId"/> is null).
        /// Keys with an empty value are deleted. Publishes a single SeoKeyValueSavedNotification afterwards.
        /// </summary>
        void SaveValues(IDictionary<string, string?> values, Guid? domainId);
    }
}
