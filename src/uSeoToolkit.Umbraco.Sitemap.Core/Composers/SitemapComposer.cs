using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Collections;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.DisplayProviders;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using uSeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using uSeoToolkit.Umbraco.Sitemap.Core.Components;
using uSeoToolkit.Umbraco.Sitemap.Core.Config;
using uSeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using uSeoToolkit.Umbraco.Sitemap.Core.Constants;
using uSeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using uSeoToolkit.Umbraco.Sitemap.Core.Middleware;
using uSeoToolkit.Umbraco.Sitemap.Core.Repositories;
using uSeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Composers
{
    public class SitemapComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("uSeoToolkit:Sitemap");
            builder.Services.Configure<SitemapAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<SitemapConfig>), typeof(SitemapConfigurationService));

            var disabledModules = section?.Get<SitemapAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All)) return;

            builder.Components().Append<EnableModuleComponent>();

            if (!disabledModules.Contains(DisabledModuleConstant.DocumentTypeContextApp))
            {
                builder.WithCollectionBuilder<DisplayCollectionBuilder>()
                    .Add<SitemapDocumentTypeDisplayProvider>();
            }

            builder.Services.AddScoped<ISitemapGenerator, SitemapGenerator>();
            builder.Services.AddScoped<ISitemapIndexGenerator, SitemapIndexGenerator>();
            builder.Services.AddUnique<ISitemapService, SitemapService>();
            builder.Services.AddUnique<ISitemapPageTypeRepository, SitemapPageTypeRepository>();

            if (!disabledModules.Contains(DisabledModuleConstant.Middleware))
            {
                builder.Services.Configure<UmbracoPipelineOptions>(options =>
                {
                    options.AddFilter(new UmbracoPipelineFilter(
                        "uSeoToolkit Sitemap",
                        applicationBuilder => { applicationBuilder.UseMiddleware<SitemapMiddleware>(); },
                        applicationBuilder => { },
                        applicationBuilder => { }
                    ));
                });
            }
        }
    }
}
