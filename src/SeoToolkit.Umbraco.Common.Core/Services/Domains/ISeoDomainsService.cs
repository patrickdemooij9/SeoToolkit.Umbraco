using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public interface ISeoDomainsService
    {
        SeoDomainCollection[] GetAll();
        SeoDomainCollection? Get(int id);
        SeoDomainCollection? GetByDomain(int umbracoDomain);

        int Save(SeoDomainCollection collection);
    }
}
