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
using uSeoToolkit.Umbraco.Redirects.Core.Components;
using uSeoToolkit.Umbraco.Redirects.Core.Config;
using uSeoToolkit.Umbraco.Redirects.Core.Config.Models;
using uSeoToolkit.Umbraco.Redirects.Core.Controllers;
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
            var section = builder.Config.GetSection("uSeoToolkit:Redirects");
            builder.Services.Configure<RedirectsAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<RedirectsConfigModel>), typeof(RedirectsConfigurationService));

            var disabledModules = section?.Get<RedirectsAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Trees().RemoveTreeController<RedirectsTreeController>();
                return;
            }

            if (disabledModules.Contains(DisabledModuleConstant.SectionTree))
            {
                builder.Trees().RemoveTreeController<RedirectsTreeController>();
            }

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddUnique<IRedirectsRepository, RedirectsRepository>();
            builder.Services.AddUnique<IRedirectsService, RedirectsService>();

            if (!disabledModules.Contains(DisabledModuleConstant.Middleware))
            {
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
}
