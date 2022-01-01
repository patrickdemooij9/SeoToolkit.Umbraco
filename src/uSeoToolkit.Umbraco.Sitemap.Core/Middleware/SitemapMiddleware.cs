using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using uSeoToolkit.Umbraco.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using uSeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using uSeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using uSeoToolkit.Umbraco.Sitemap.Core.Utils;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Middleware
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly ISitemapIndexGenerator _sitemapIndexGenerator;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISettingsService<SitemapConfig> _settingsService;

        public SitemapMiddleware(RequestDelegate next,
            ISitemapGenerator sitemapGenerator,
            ISitemapIndexGenerator sitemapIndexGenerator,
            IUmbracoContextFactory umbracoContextFactory,
            ISettingsService<SitemapConfig> settingsService)
        {
            _next = next;
            _sitemapGenerator = sitemapGenerator;
            _sitemapIndexGenerator = sitemapIndexGenerator;
            _umbracoContextFactory = umbracoContextFactory;
            _settingsService = settingsService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value?.EndsWith("/sitemap.xml", StringComparison.OrdinalIgnoreCase) != true)
            {
                await _next.Invoke(context);
                return;
            }

            XDocument doc = null;
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                //If domain is null, we are either at root or we don't have any domains on the website anyway.
                var domains = ctx.UmbracoContext.Domains.GetAll(false).ToArray();
                if (domains.Length == 0)
                {
                    doc = _sitemapGenerator.Generate(new SitemapGeneratorOptions(null, ctx.UmbracoContext.Domains.DefaultCulture, false));
                }
                else
                {
                    var domain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                    if (domain is null)
                    {
                        doc = _sitemapIndexGenerator.Generate();
                    }
                    else
                    {
                        var rootNode = ctx.UmbracoContext.Content.GetById(domain.ContentId);
                        if (rootNode is null)
                        {
                            await _next.Invoke(context);
                            return;
                        }

                        doc = _sitemapGenerator.Generate(new SitemapGeneratorOptions(rootNode, domain.Culture, _settingsService.GetSettings().ShowAlternatePages));
                    }
                }
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/xml";

            using (var writer = new UTF8StringWriter())
            {
                await doc.SaveAsync(writer, SaveOptions.None, CancellationToken.None);
                await context.Response.WriteAsync(writer.ToString());
            }
        }
    }
}
