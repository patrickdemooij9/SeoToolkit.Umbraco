using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using System;
using System.Linq;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public class SeoDomainResolver : ISeoDomainResolver
    {
        private readonly ISeoDomainsService _seoDomainsService;
        private readonly IDomainService _domainService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SeoDomainResolver(ISeoDomainsService seoDomainsService, IDomainService domainService, IUmbracoContextFactory umbracoContextFactory, IHttpContextAccessor httpContextAccessor)
        {
            _seoDomainsService = seoDomainsService;
            _domainService = domainService;
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

            var domainKey = _domainService.GetById(domain.Id)?.Key; //TODO: I should check if this touches the database every time or not...
            if (domainKey is null) return null;

            return _seoDomainsService.GetByDomain(domainKey.Value);
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
