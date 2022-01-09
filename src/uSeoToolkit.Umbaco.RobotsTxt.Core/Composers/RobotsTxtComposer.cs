using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Components;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Middleware;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Composers
{
    public class RobotsTxtComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<EnableModuleComponent>();

            builder.Services.Configure<UmbracoPipelineOptions>(options => {
                options.AddFilter(new UmbracoPipelineFilter(
                    "uSeoToolkit Robots.txt",
                    applicationBuilder => { applicationBuilder.UseMiddleware<RobotsTxtMiddleware>(); },
                    applicationBuilder => { },
                    applicationBuilder => { }
                ));
            });
        }
    }
}
