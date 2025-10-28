using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Core.Connectors
{
    public class RobotsSitemapProvider : IRobotsTxtSitemapProvider
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISeoDomainResolver _seoDomainResolver;

        public RobotsSitemapProvider(IUmbracoContextFactory umbracoContextFactory, ISeoDomainResolver seoDomainResolver)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _seoDomainResolver = seoDomainResolver;
        }

        public IEnumerable<string> GetSitemapUrls(HttpRequest request)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();

            var seoDomain = _seoDomainResolver.ResolveDomain();
            if (seoDomain != null)
            {
                domains = domains.Where(it => seoDomain.DomainIds.Contains(it.Id)).ToArray();
            }
            
            var baseUri = new Uri(request.GetEncodedUrl());
            if (domains.Length == 0)
            {
                yield return $"{baseUri.GetLeftPart(UriPartial.Authority).TrimEnd('/')}/sitemap.xml";
            }
            else
            {
                foreach (var domain in domains)
                {
                    var url = domain.Name.StartsWith('/') ? new Uri(baseUri, domain.Name).ToString() : domain.Name;
                    yield return $"{url.TrimEnd('/')}/sitemap.xml";
                }
            }
        }
    }
}
