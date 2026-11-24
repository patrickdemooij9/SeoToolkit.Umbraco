using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Caching;
using SeoToolkit.Umbraco.Sitemap.Core.Collections;
using SeoToolkit.Umbraco.Sitemap.Core.Common.CollectionProviders;
using SeoToolkit.Umbraco.Sitemap.Core.Common.DisplayProviders;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapIndexGenerator;
using SeoToolkit.Umbraco.Sitemap.Core.Components;
using SeoToolkit.Umbraco.Sitemap.Core.Config;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Middleware;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Repositories;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;

namespace SeoToolkit.Umbraco.Sitemap.Core.Composers
{
    public class SitemapComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Sitemap");
            builder.Services.Configure<SitemapAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<SitemapConfig>), typeof(SitemapConfigurationService));

            var disabledModules = section?.Get<SitemapAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                return;
            }

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddScoped<ISitemapGenerator, SitemapGenerator>();
            builder.Services.AddScoped<ISitemapIndexGenerator, SitemapIndexGenerator>();
            builder.Services.AddUnique<ISitemapService, SitemapService>();
            builder.Services.AddUnique<ISitemapPageTypeRepository, SitemapPageTypeRepository>();
            builder.Services.AddUnique<ISitemapContentRepository, SitemapContentRepository>();

            // Content pages always come first, custom collections are appended by other composers.
            builder.SitemapCollections().Insert<ContentSitemapCollectionProvider>();

            builder.AddNotificationHandler<ContentCacheRefresherNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<DomainCacheRefresherNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<PublicAccessCacheRefresherNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<LanguageCacheRefresherNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<SitemapContentChangedNotification, SitemapCacheClearNotificationHandler>();
            builder.AddNotificationHandler<SitemapPageSettingsSavedNotification, SitemapCacheClearNotificationHandler>();

            if (!disabledModules.Contains(DisabledModuleConstant.Middleware))
            {
                builder.Services.Configure<UmbracoPipelineOptions>(options =>
                {
                    options.AddFilter(new UmbracoPipelineFilter(
                        "SeoToolkit Sitemap",
                        applicationBuilder => { applicationBuilder.UseMiddleware<SitemapMiddleware>(); },
                        applicationBuilder => { },
                        applicationBuilder => { }
                    ));
                });
            }
        }
    }
}
