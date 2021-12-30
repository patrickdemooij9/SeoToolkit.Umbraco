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
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Middleware
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IDomainService _domainService;

        public SitemapMiddleware(RequestDelegate next,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISitemapGenerator sitemapGenerator,
            IUmbracoContextFactory umbracoContextFactory,
            IDomainService domainService)
        {
            _next = next;
            _umbracoContextAccessor = umbracoContextAccessor;
            _sitemapGenerator = sitemapGenerator;
            _umbracoContextFactory = umbracoContextFactory;
            _domainService = domainService;
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
                    doc = _sitemapGenerator.Generate(ctx.UmbracoContext.Domains.DefaultCulture);
                }
                else
                {
                    var domain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                    if (domain is null)
                    {
                        await _next.Invoke(context);
                        return;
                    }
                    var rootNode = ctx.UmbracoContext.Content.GetById(domain.ContentId);
                    if (rootNode is null)
                    {
                        await _next.Invoke(context);
                        return;
                    }

                    doc = _sitemapGenerator.Generate(rootNode, domain.Culture);
                }
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/xml";

            using (var writer = new StringWriter())
            {
                await doc.SaveAsync(writer, SaveOptions.None, CancellationToken.None);
                await context.Response.WriteAsync(writer.ToString());
            }
        }
    }
}
