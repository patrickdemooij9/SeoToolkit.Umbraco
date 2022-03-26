using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Redirects.Core.Components;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Middleware;
using uSeoToolkit.Umbraco.Redirects.Core.Repositories;
using uSeoToolkit.Umbraco.Redirects.Core.Services;

namespace uSeoToolkit.Umbraco.Redirects.Core.Composers
{
    public class RedirectsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddUnique<IRedirectsRepository, RedirectsRepository>();
            builder.Services.AddUnique<IRedirectsService, RedirectsService>();

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "uSeoToolkitRedirects",
                    applicationBuilder =>
                    {
                        applicationBuilder.UseMiddleware<RedirectMiddleware>();
                    },
                    applicationBuilder =>
                    {
                    },
                    applicationBuilder => { }
                ));
            });
        }
    }
}
