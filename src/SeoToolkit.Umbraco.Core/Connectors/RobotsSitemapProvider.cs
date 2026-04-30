using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using SeoToolkit.Umbraco.Core.SeoSettings;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.Core.Connectors
{
    public class RobotsSitemapProvider : IRobotsTxtSitemapProvider
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly ISeoKeyValueService _seoKeyValueService;

        public RobotsSitemapProvider(IUmbracoContextFactory umbracoContextFactory,
            ISeoDomainResolver seoDomainResolver,
            ISeoKeyValueService seoKeyValueService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _seoDomainResolver = seoDomainResolver;
            _seoKeyValueService = seoKeyValueService;
        }

        public IEnumerable<string> GetSitemapUrls(HttpRequest request)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var domains = ctx.UmbracoContext.Domains.GetAll(includeWildcards: false).ToArray();

            var seoDomain = _seoDomainResolver.ResolveDomain();
            if (!bool.TryParse(_seoKeyValueService.GetValue(AutomaticSitemapInRobotsTxtSeoSetting.SettingKey), out var result) || !result)
            {
                yield break;
            }

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
                    if (!url.StartsWith("http"))
                    {
                        url = $"https://{url}";
                    }
                    yield return $"{url.TrimEnd('/')}/sitemap.xml";
                }
            }
        }
    }
}
