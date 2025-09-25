using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.Domains
{
    public interface ISeoDomainsRepository
    {
        SeoDomainCollection[] GetAll();
        SeoDomainCollection? Get(int id);

        void Save(SeoDomainCollection collection);
    }
}
