using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Constants;
using uSeoToolkit.Umbraco.Common.Core.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Components;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Config;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Config.Models;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Controllers;
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
            var section = builder.Config.GetSection("uSeoToolkit:RobotsTxt");
            builder.Services.Configure<RobotsTxtAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<RobotsTxtConfigModel>), typeof(RobotsTxtConfigurationService));

            var disabledModules = section?.Get<RobotsTxtAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                builder.Trees().RemoveTreeController<RobotsTxtTreeController>();
                return;
            }

            if (disabledModules.Contains(DisabledModuleConstant.SectionTree))
                builder.Trees().RemoveTreeController<RobotsTxtTreeController>();

            builder.Services.AddUnique<IRobotsTxtRepository, RobotsTxtRepository>();
            builder.Services.AddUnique<IRobotsTxtService, RobotsTxtService>();
            builder.Services.AddUnique<IRobotsTxtValidator, DefaultRobotsTxtValidator>();

            builder.Components().Append<EnableModuleComponent>();

            if (!disabledModules.Contains(DisabledModuleConstant.Middleware))
            {
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
}
