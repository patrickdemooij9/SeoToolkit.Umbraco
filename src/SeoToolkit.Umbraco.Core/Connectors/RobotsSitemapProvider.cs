﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
