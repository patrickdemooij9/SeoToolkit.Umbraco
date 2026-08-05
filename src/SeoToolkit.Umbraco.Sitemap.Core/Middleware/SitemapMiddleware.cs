using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using SeoToolkit.Umbraco.Sitemap.Core.Config;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Utils;
using Microsoft.Extensions.Logging;

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
            ISitemapIndexGenerator sitemapIndexGenerator,
            ISeoDomainResolver seoDomainResolver)
        {
            if (context.Request.Path.Value?.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) != true)
            {
                await _next.Invoke(context);
                return;
            }

            var settings = _sitemapConfigurationService.GetSettings();
            var seoDomain = seoDomainResolver.ResolveDomain();
            var baseUrl = seoDomain?.BaseUrl;

            var isSitemapRequest = context.Request.Path.Value.EndsWith("/sitemap.xml", StringComparison.OrdinalIgnoreCase);
            var isConfiguredIndexRequest = !string.IsNullOrWhiteSpace(settings.SitemapIndexPath)
                && IsSitemapIndexRequest(context.Request.Path, settings.SitemapIndexPath);

            if (!isSitemapRequest && !isConfiguredIndexRequest)
            {
                await _next.Invoke(context);
                return;
            }

            XDocument doc = null;
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                //If domain is null, we are either at root or we don't have any domains on the website anyway.
                var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();
                if (domains.Length == 0 || settings.StructureMode == StructureMode.OnlyRoot)
                {
                    doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(null, ctx.UmbracoContext.Domains.DefaultCulture, baseUrl));
                }
                else if (isConfiguredIndexRequest)
                {
                    // A dedicated index path is configured and this request targets it. Serve the index here so the
                    // root /sitemap.xml stays free to serve the default culture's sitemap (see SitemapIndexPath).
                    doc = sitemapIndexGenerator.Generate();
                }
                else
                {
                    var domain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                    if (domain is null)
                    {
                        if (domains.Length == 1) // No point showing a sitemap index if there is only 1 domain.
                        {
                            await _next.Invoke(context);
                            return;
                        }
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

                        doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(rootNode, domain.Culture, baseUrl));
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

        //Matches requests targeting the configured index path. A value ending in ".xml" is treated as a full filename
        //("sitemap-index.xml" -> "/sitemap-index.xml"), otherwise as a folder segment ("sitemap-index" -> "/sitemap-index/sitemap.xml").
        private static bool IsSitemapIndexRequest(PathString requestPath, string sitemapIndexPath)
        {
            var trimmed = sitemapIndexPath.Trim('/');
            var expected = trimmed.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                ? $"/{trimmed}"
                : $"/{trimmed}/sitemap.xml";
            return string.Equals(requestPath.Value?.TrimEnd('/'), expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
