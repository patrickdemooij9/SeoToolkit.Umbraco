using Lucene.Net.Analysis.Hunspell;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository
{
    public interface ISeoKeyValueRepository
    {
        void Set(string key, string value, Guid? domainId);
        string? Get(string key, Guid? domainId);
        Dictionary<string, string> Get(Guid? domainId);
        void Delete(string key, Guid? domainId);
    }
}
