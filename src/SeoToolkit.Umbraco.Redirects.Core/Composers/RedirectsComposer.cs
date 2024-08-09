using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Redirects.Core.Components;
using SeoToolkit.Umbraco.Redirects.Core.Config;
using SeoToolkit.Umbraco.Redirects.Core.Config.Models;
using SeoToolkit.Umbraco.Redirects.Core.Controllers;
using SeoToolkit.Umbraco.Redirects.Core.Helpers;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Middleware;
using SeoToolkit.Umbraco.Redirects.Core.Repositories;
using SeoToolkit.Umbraco.Redirects.Core.Services;

namespace SeoToolkit.Umbraco.Redirects.Core.Composers
{
    public class RedirectsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Redirects");
            builder.Services.Configure<RedirectsAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<RedirectsConfigModel>), typeof(RedirectsConfigurationService));

            var disabledModules = section?.Get<RedirectsAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
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
            builder.Services.AddTransient<RedirectsImportHelper>();

            if (!disabledModules.Contains(DisabledModuleConstant.Middleware))
            {
                builder.Services.Configure<UmbracoPipelineOptions>(options =>
                {
                    options.AddFilter(new UmbracoPipelineFilter(
                        "SeoToolkitRedirects",
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
