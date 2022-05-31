using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler;
using SeoToolkit.Umbraco.SiteAudit.Core.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using SeoToolkit.Umbraco.SiteAudit.Core.Extensions;
using SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler;
using SeoToolkit.Umbraco.SiteAudit.Core.Hubs;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.NotificationHandlers;
using SeoToolkit.Umbraco.SiteAudit.Core.Notifications;
using SeoToolkit.Umbraco.SiteAudit.Core.Repositories;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Composers
{
    public class SiteAuditComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ISiteAuditRepository), typeof(SiteAuditDatabaseRepository));
            builder.Services.AddSingleton(typeof(ISiteCrawlerFactory), typeof(DefaultSiteCrawlerFactory));
            builder.Services.AddSingleton(typeof(SiteAuditService), typeof(SiteAuditService));
            builder.Services.AddSingleton(typeof(ISiteCheckService), typeof(SiteCheckService));
            builder.Services.AddSingleton(typeof(SiteAuditHubClientService));
            builder.Services.AddSingleton(typeof(ISettingsService<SiteAuditConfigModel>), typeof(SiteAuditConfigurationService));
            builder.Services.AddSingleton(typeof(ISiteCheckRepository), typeof(SiteCheckDatabaseRepository));
            builder.Services.AddSingleton(typeof(ISiteAuditScheduler), typeof(SiteAuditScheduler));

            builder.Services.AddHostedService<ScheduledSiteAuditTask>();

            builder.WithCollectionBuilder<SiteAuditCheckCollectionBuilder>()
                .Append<BrokenLinkCheck>()
                .Append<MissingTitleCheck>()
                .Append<MissingDescriptionCheck>()
                .Append<BrokenImageCheck>()
                .Append<MissingImageAltCheck>();

            builder.AddNotificationHandler<SiteAuditUpdatedNotification, SiteAuditUpdateNotificationHandler>();

            builder.Services.Configure<SiteAuditAppSettingsModel>(builder.Config.GetSection("SeoToolkit:SiteAudit"));

            builder.Services.AddSingleton<SiteAuditHubRoutes>();
            builder.Services.AddSignalR();
            builder.Services.AddHubSignalR();

            builder.Services.AddHttpClient<BrokenImageCheck>()
                .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                });
            builder.Services.AddHttpClient<BrokenLinkCheck>()
                .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                });
        }
    }
}
