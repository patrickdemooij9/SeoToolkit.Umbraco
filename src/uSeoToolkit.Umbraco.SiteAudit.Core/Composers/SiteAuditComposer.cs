using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.SiteAudit.Core.Checks;
using uSeoToolkit.Umbraco.SiteAudit.Core.Collections;
using uSeoToolkit.Umbraco.SiteAudit.Core.Components;
using uSeoToolkit.Umbraco.SiteAudit.Core.Config;
using uSeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using uSeoToolkit.Umbraco.SiteAudit.Core.Extensions;
using uSeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler;
using uSeoToolkit.Umbraco.SiteAudit.Core.Hubs;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using uSeoToolkit.Umbraco.SiteAudit.Core.NotificationHandlers;
using uSeoToolkit.Umbraco.SiteAudit.Core.Notifications;
using uSeoToolkit.Umbraco.SiteAudit.Core.Repositories;
using uSeoToolkit.Umbraco.SiteAudit.Core.Services;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Composers
{
    public class SiteAuditComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped(typeof(ISiteAuditRepository), typeof(SiteAuditDatabaseRepository));
            builder.Services.AddScoped(typeof(ISiteCrawlerFactory), typeof(DefaultSiteCrawlerFactory));
            builder.Services.AddScoped(typeof(SiteAuditService), typeof(SiteAuditService));
            builder.Services.AddScoped(typeof(ISiteCheckService), typeof(SiteCheckService));
            builder.Services.AddSingleton(typeof(SiteAuditHubClientService), typeof(SiteAuditHubClientService));
            builder.Services.AddScoped(typeof(ISettingsService<SiteAuditConfigModel>), typeof(SiteAuditConfigurationService));
            builder.Services.AddScoped(typeof(ISiteCheckRepository), typeof(SiteCheckDatabaseRepository));

            builder.WithCollectionBuilder<SiteAuditCheckCollectionBuilder>()
                .Append<BrokenLinkCheck>()
                .Append<MissingTitleCheck>()
                .Append<MissingDescriptionCheck>()
                .Append<BrokenImageCheck>()
                .Append<MissingImageAltCheck>();

            builder.Components().Append<SiteAuditDatabaseComponent>();

            builder.AddNotificationHandler<SiteAuditUpdatedNotification, SiteAuditUpdateNotificationHandler>();

            builder.Services.Configure<SiteAuditAppSettingsModel>(builder.Config.GetSection("uSeoToolkit:SiteAudit"));

            builder.Services.AddUnique<SiteAuditHubRoutes>();
            builder.Services.AddSignalR();
            builder.Services.AddHubSignalR();
        }
    }
}
