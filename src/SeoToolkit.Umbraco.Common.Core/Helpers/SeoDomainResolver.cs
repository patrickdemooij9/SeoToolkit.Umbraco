using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public class SeoDomainResolver : ISeoDomainResolver
    {
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SeoDomainResolver(ISeoDomainsService seoDomainsService, IUmbracoContextFactory umbracoContextFactory)
        {
            _seoDomainsService = seoDomainsService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public SeoDomainCollection? ResolveSeoDomain(Uri requestUrl)
        {
            using var contextReference = _umbracoContextFactory.EnsureUmbracoContext();
            if (contextReference.UmbracoContext is null) return null;

            var domains = contextReference.UmbracoContext.Domains.GetAll(includeWildcards: false);
            var domain = DomainUtilities.SelectDomain(domains, requestUrl);
            if (domain is null) return null;

            return _seoDomainsService.GetByDomain(domain.Id);
        }
    }
}
