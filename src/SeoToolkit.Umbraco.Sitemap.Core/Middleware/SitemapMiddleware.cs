﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using SeoToolkit.Umbraco.Sitemap.Core.Config;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Utils;

namespace SeoToolkit.Umbraco.Sitemap.Core.Middleware
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISettingsService<SitemapConfig> _sitemapConfigurationService;

        public SitemapMiddleware(RequestDelegate next,
            IUmbracoContextFactory umbracoContextFactory,
            ISettingsService<SitemapConfig> sitemapConfigurationService)
        {
            _next = next;
            _umbracoContextFactory = umbracoContextFactory;
            _sitemapConfigurationService = sitemapConfigurationService;
        }

        public async Task Invoke(HttpContext context,
            ISitemapGenerator sitemapGenerator,
            ISitemapIndexGenerator sitemapIndexGenerator)
        {
            if (context.Request.Path.Value?.EndsWith("/sitemap.xml", StringComparison.OrdinalIgnoreCase) != true)
            {
                await _next.Invoke(context);
                return;
            }

            var settings = _sitemapConfigurationService.GetSettings();

            XDocument doc = null;
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                //If domain is null, we are either at root or we don't have any domains on the website anyway.
                var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();
                if (domains.Length <= 1 || settings.StructureMode == StructureMode.OnlyRoot)
                {
                    doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(null, ctx.UmbracoContext.Domains.DefaultCulture));
                }
                else
                {
                    var domain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                    if (domain is null)
                    {
                        doc = sitemapIndexGenerator.Generate();
                    }
                    else
                    {
                        var rootNode = ctx.UmbracoContext.Content.GetById(domain.ContentId);
                        if (rootNode is null)
                        {
                            await _next.Invoke(context);
                            return;
                        }

                        doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(rootNode, domain.Culture));
                    }
                }
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = settings.ReturnContentType;

            using (var writer = new UTF8StringWriter())
            {
                await doc.SaveAsync(writer, SaveOptions.None, CancellationToken.None);
                await context.Response.WriteAsync(writer.ToString());
            }
        }
    }
}
