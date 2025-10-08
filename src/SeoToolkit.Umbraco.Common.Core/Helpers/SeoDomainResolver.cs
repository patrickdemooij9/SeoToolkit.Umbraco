using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using System;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public class SeoDomainResolver : ISeoDomainResolver
    {
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SeoDomainResolver(ISeoDomainsService seoDomainsService, IUmbracoContextFactory umbracoContextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _seoDomainsService = seoDomainsService;
            _umbracoContextFactory = umbracoContextFactory;
            _httpContextAccessor = httpContextAccessor;
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

        public SeoDomainCollection? ResolveDomain()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is null) return null;
            var uri = new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
            return ResolveSeoDomain(uri);
        }
    }
}
