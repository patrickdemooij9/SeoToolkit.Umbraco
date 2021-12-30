using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using uSeoToolkit.Umbraco.Sitemap.Core.Components;
using uSeoToolkit.Umbraco.Sitemap.Core.Controllers;
using uSeoToolkit.Umbraco.Sitemap.Core.Middleware;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Composers
{
    public class SitemapComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            //builder.Services.AddUnique<SitemapRegistrationComponent>();

            builder.Services.AddUnique<ISitemapGenerator, SitemapGenerator>();

            builder.Services.Configure<UmbracoPipelineOptions>(options => {
                options.AddFilter(new UmbracoPipelineFilter(
                    "uSeoToolkit Sitemap",
                    applicationBuilder => {},
                    applicationBuilder => { applicationBuilder.UseMiddleware<SitemapMiddleware>(); },
                    applicationBuilder => { }
                ));
            });
        }
    }
}
