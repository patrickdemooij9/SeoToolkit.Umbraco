using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Repositories.Domains;

namespace SeoToolkit.Umbraco.Common.Core.Services.Domains
{
    public class SeoDomainsService : ISeoDomainsService
    {
        private readonly ISeoDomainsRepository _seoDomainsRepository;

        public SeoDomainsService(ISeoDomainsRepository seoDomainsRepository)
        {
            _seoDomainsRepository = seoDomainsRepository;
        }

        public SeoDomainCollection[] GetAll()
        {
            return _seoDomainsRepository.GetAll();
        }

        public void Save(SeoDomainCollection collection)
        {
            _seoDomainsRepository.Save(collection);
        }
    }
}
