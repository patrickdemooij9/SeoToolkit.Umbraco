using System;
using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.Domains
{
    public interface ISeoDomainsRepository
    {
        SeoDomainCollection[] GetAll();
        SeoDomainCollection? Get(Guid id);

        Guid Save(SeoDomainCollection collection);
        void Delete(Guid domainId);
    }
}
