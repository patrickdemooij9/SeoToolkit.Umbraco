using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Common.Shortcuts;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Components;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Extensions;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Middleware;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Repositories;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Services;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Composers
{
    public class RobotsTxtComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.RobotsTxtShortcuts()
                .Append<DisallowAllShortcut>();

            builder.Services.AddUnique<IRobotsTxtRepository, RobotsTxtRepository>();
            builder.Services.AddUnique<IRobotsTxtService, RobotsTxtService>();
            builder.Services.AddUnique<IRobotsTxtValidator, DefaultRobotsTxtValidator>();

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
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
