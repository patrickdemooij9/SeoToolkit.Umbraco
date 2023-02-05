using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Polly;
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

        public RobotsSitemapProvider(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public IEnumerable<string> GetSitemapUrls(HttpRequest request)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();
            if (domains.Length == 0)
            {
                yield return $"{new Uri(request.GetEncodedUrl()).GetLeftPart(UriPartial.Authority).TrimEnd('/')}/sitemap.xml";
            }
            else
            {
                foreach (var domain in domains)
                {
                    yield return $"{domain.Name.TrimEnd('/')}/sitemap.xml";
                }
            }
        }
    }
}
