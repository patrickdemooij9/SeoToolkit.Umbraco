using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Common.Scheduler;
using SeoToolkit.Umbraco.SiteAudit.Core.Components;
using SeoToolkit.Umbraco.SiteAudit.Core.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Notifications;
using SeoToolkit.Umbraco.SiteAudit.Core.Repositories;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using SeoToolkit.Umbraco.SiteAudit.Core.Startup;
using System;
using System.Linq;
using System.Net.Http;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Composers
{
    public class SiteAuditComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:SiteAudit");
            builder.Services.Configure<SiteAuditAppSettingsModel>(section);

            var disabledModules = section?.Get<SiteAuditAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                return;
            }

            builder.Services.AddSingleton(typeof(ISiteAuditRepository), typeof(SiteAuditDatabaseRepository));
            builder.Services.AddSingleton(typeof(ISiteCrawlerFactory), typeof(DefaultSiteCrawlerFactory));
            builder.Services.AddSingleton(typeof(SiteAuditService), typeof(SiteAuditService));
            builder.Services.AddSingleton(typeof(ISiteCheckService), typeof(SiteCheckService));
            builder.Services.AddSingleton(typeof(ISettingsService<SiteAuditConfigModel>), typeof(SiteAuditConfigurationService));
            builder.Services.AddSingleton(typeof(ISiteCheckRepository), typeof(SiteCheckDatabaseRepository));
            builder.Services.AddSingleton(typeof(ISiteAuditScheduler), typeof(SiteAuditScheduler));

            if (!disabledModules.Contains(DisabledModuleConstant.SectionTree))
            {
                builder.WithCollectionBuilder<SeoTreeSectionCollectionBuilder>()
                .Add<SiteAuditTreeSection>();
            }

            builder.WithCollectionBuilder<SiteAuditCheckCollectionBuilder>()
                .Append<BrokenLinkCheck>()
                .Append<MissingTitleCheck>()
                .Append<MissingDescriptionCheck>()
                .Append<MissingH1Check>()
                .Append<MissingCanonicalCheck>()
                .Append<ThinContentCheck>()
                .Append<OrphanedPageCheck>()
                .Append<CoreWebVitalsCheck>()
                .Append<BrokenImageCheck>()
                .Append<MissingImageAltCheck>();

            builder.WithCollectionBuilder<SeoDisplayCollectionBuilder>()
                .Add<SiteAuditDisplayProvider>();

            builder.Services.AddHttpClient<BrokenImageCheck>()
                .ConfigurePrimaryHttpMessageHandler(x =>
                {
                    var allowInvalidCerts = x.GetRequiredService<ISettingsService<SiteAuditConfigModel>>().GetSettings().AllowInvalidCerts;

                    return new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => allowInvalidCerts
                    };
                });

            builder.Services.AddHttpClient<BrokenLinkCheck>()
                .ConfigurePrimaryHttpMessageHandler(x =>
                {
                    var allowInvalidCerts = x.GetRequiredService<ISettingsService<SiteAuditConfigModel>>().GetSettings().AllowInvalidCerts;

                    return new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => allowInvalidCerts
                    };
                });

            builder.Components().Append<EnableModuleComponent>();
        }
    }
}
