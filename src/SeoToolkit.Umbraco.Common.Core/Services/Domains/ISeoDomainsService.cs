using SeoToolkit.Umbraco.Common.Core.Models.Business;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public interface ISeoDomainsService
    {
        SeoDomainCollection[] GetAll();

        void Save(SeoDomainCollection collection);
    }
}
