using System;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository
{
    public interface ISeoKeyValueRepository
    {
        void Set(string key, string value, Guid? domainId);
        string? Get(string key, Guid? domainId);
        void Delete(string key, Guid? domainId);
    }
}
