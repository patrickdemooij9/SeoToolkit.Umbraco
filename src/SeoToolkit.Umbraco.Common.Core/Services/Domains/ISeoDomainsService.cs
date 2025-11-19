using System;
using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public interface ISeoDomainsService
    {
        SeoDomainCollection[] GetAll();
        SeoDomainCollection? Get(Guid id);
        SeoDomainCollection? GetByDomain(int umbracoDomain);

        Guid Save(SeoDomainCollection collection);
        void Delete(Guid domainId);
    }
}
