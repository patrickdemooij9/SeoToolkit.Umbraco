using System;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService
{
    public interface ISeoKeyValueService
    {
        public string? GetValue(string key);
    }
}
