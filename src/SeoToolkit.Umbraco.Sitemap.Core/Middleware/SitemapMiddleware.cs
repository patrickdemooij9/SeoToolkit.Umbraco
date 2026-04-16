using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
using Microsoft.Extensions.Logging;

namespace SeoToolkit.Umbraco.Sitemap.Core.Middleware
{
    public class SitemapMiddleware
    {
        private const int MaxUrlsPerSitemap = 50000;
        private static readonly Regex SplitSitemapRegex = new(@"/sitemap-(\d+)\.xml$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            if (!TryParseSitemapRequest(context.Request.Path.Value, out var splitSitemapPageNumber))
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
                if (domains.Length == 0 || settings.StructureMode == StructureMode.OnlyRoot)
                {
                    doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(null, ctx.UmbracoContext.Domains.DefaultCulture));
                    doc = ResolveSplitSitemapDocument(doc, context, splitSitemapPageNumber);
                }
                else
                {
                    var domain = DomainUtilities.SelectDomain(domains, new Uri(context.Request.GetEncodedUrl()));
                    if (domain is null)
                    {
                        if (splitSitemapPageNumber.HasValue)
                        {
                            await _next.Invoke(context);
                            return;
                        }

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

                        doc = sitemapGenerator.Generate(new SitemapGeneratorOptions(rootNode, domain.Culture));
                        doc = ResolveSplitSitemapDocument(doc, context, splitSitemapPageNumber);
                    }
                }
            }

            if (doc is null)
            {
                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = settings.ReturnContentType;

            using (var writer = new UTF8StringWriter())
            {
                await doc.SaveAsync(writer, SaveOptions.None, CancellationToken.None);
                await context.Response.WriteAsync(writer.ToString());
            }
        }

        private static bool TryParseSitemapRequest(string? path, out int? splitSitemapPageNumber)
        {
            splitSitemapPageNumber = null;
            if (path?.EndsWith("/sitemap.xml", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            if (path is null)
            {
                return false;
            }

            var splitSitemapMatch = SplitSitemapRegex.Match(path);
            if (!splitSitemapMatch.Success)
            {
                return false;
            }

            if (!int.TryParse(splitSitemapMatch.Groups[1].Value, out var pageNumber))
            {
                return false;
            }

            splitSitemapPageNumber = pageNumber;
            return pageNumber > 0;
        }

        private static XDocument? ResolveSplitSitemapDocument(XDocument sitemapDocument, HttpContext context, int? splitSitemapPageNumber)
        {
            var root = sitemapDocument.Root;
            if (root is null)
            {
                return sitemapDocument;
            }

            var urls = root.Elements().ToArray();
            if (urls.Length <= MaxUrlsPerSitemap)
            {
                return splitSitemapPageNumber.HasValue ? null : sitemapDocument;
            }

            var totalSplitSitemaps = (urls.Length + MaxUrlsPerSitemap - 1) / MaxUrlsPerSitemap;
            if (!splitSitemapPageNumber.HasValue)
            {
                return BuildSitemapIndex(context, totalSplitSitemaps);
            }

            if (splitSitemapPageNumber.Value > totalSplitSitemaps)
            {
                return null;
            }

            var urlsForSitemap = urls.Skip((splitSitemapPageNumber.Value - 1) * MaxUrlsPerSitemap).Take(MaxUrlsPerSitemap);
            return BuildSitemapDocument(root.Name, root.Attributes(), urlsForSitemap);
        }

        private static XDocument BuildSitemapDocument(XName rootName, IEnumerable<XAttribute> rootAttributes, IEnumerable<XElement> urls)
        {
            return new XDocument(new XElement(rootName, rootAttributes, urls));
        }

        private static XDocument BuildSitemapIndex(HttpContext context, int totalSplitSitemaps)
        {
            var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var lastSlashIndex = requestPath.LastIndexOf('/');
            var sitemapPathBase = lastSlashIndex >= 0 ? requestPath[..lastSlashIndex] : string.Empty;
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{sitemapPathBase.TrimEnd('/')}";

            var sitemapIndexElement = new XElement(ns + "sitemapindex");
            for (var i = 1; i <= totalSplitSitemaps; i++)
            {
                sitemapIndexElement.Add(
                    new XElement(ns + "sitemap",
                        new XElement(ns + "loc", $"{baseUrl}/sitemap-{i}.xml")));
            }

            return new XDocument(sitemapIndexElement);
        }
    }
}
