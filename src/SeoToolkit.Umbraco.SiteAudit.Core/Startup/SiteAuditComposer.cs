using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Seo;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Extensions;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.BackgroundTasks;
using SeoToolkit.Umbraco.SiteAudit.Core.Components;
using SeoToolkit.Umbraco.SiteAudit.Core.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.Repositories;
using SeoToolkit.Umbraco.SiteAudit.Core.Services;
using SeoToolkit.Umbraco.SiteAudit.Core.Startup;
using System;
using System.Linq;
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

            builder.Services.AddSingleton(typeof(ISettingsService<SiteAuditConfigModel>), typeof(SiteAuditConfigurationService));

            if (!disabledModules.Contains(DisabledModuleConstant.SectionTree))
            {
                builder.WithCollectionBuilder<SeoTreeSectionCollectionBuilder>()
                .Add<SiteAuditTreeSection>();
            }

            // The check api add-on packages compose against.
            builder.Services.AddSingleton<ISeoCheckMessageFormatter, DefaultSeoCheckMessageFormatter>();
            builder.SeoChecks();

            // The crawl pipeline. Its client never follows redirects automatically, because the
            // chain is something checks need to be able to see.
            builder.Services.AddSingleton<ICrawlEngineFactory, CrawlEngineFactory>();

            // Transient rather than singleton: it reads whatever scope is ambient at the moment
            // it is called, and holds no state of its own between calls.
            builder.Services.AddTransient<ISiteAuditRunRepository, SiteAuditRunRepository>();
            builder.Services.AddTransient<SiteAuditRunService>();
            builder.Services.AddSingleton<ISeoCheckCatalogue, SeoCheckCatalogue>();
            builder.Services.AddSingleton<SiteAuditViewModelMapper>();
            builder.Services.AddSingleton<IAuditStartingPointResolver, AuditStartingPointResolver>();

            // Runs queued audits. This is what makes an audit outlive the request that asked for
            // it - the previous scheduler was never registered, so nothing ever picked one up.
            builder.Services.AddHostedService<SiteAuditJobRunner>();
            builder.Services.AddHttpClient(CrawlEngineFactory.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(x =>
                {
                    var settings = x.GetRequiredService<ISettingsService<SiteAuditConfigModel>>().GetSettings();

                    return HttpResourceFetcher.CreateHandler(new CrawlOptions
                    {
                        AllowInvalidCertificates = settings.AllowInvalidCerts
                    });
                });

            builder.SeoChecks()
                // Indexability - whether the page can be found at all.
                .Append<NoindexCheck>()
                .Append<CanonicalMissingCheck>()
                .Append<CanonicalNonSelfReferencingCheck>()
                .Append<MultipleCanonicalsCheck>()
                .Append<MetaRefreshCheck>()
                .Append<OrphanPageCheck>()

                // Technical - transport, status codes and redirects.
                .Append<ClientErrorCheck>()
                .Append<ServerErrorCheck>()
                .Append<UnreachableCheck>()
                .Append<RedirectChainCheck>()
                .Append<RedirectLoopCheck>()
                .Append<TemporaryRedirectCheck>()
                .Append<NonHttpsCheck>()
                .Append<MixedContentCheck>()
                .Append<UrlLengthCheck>()

                // Performance.
                .Append<SlowResponseCheck>()
                .Append<LargePageCheck>()

                // On-page markup.
                .Append<TitleMissingCheck>()
                .Append<TitleLengthCheck>()
                .Append<MultipleTitlesCheck>()
                .Append<DescriptionMissingCheck>()
                .Append<DescriptionLengthCheck>()
                .Append<H1MissingCheck>()
                .Append<MultipleH1Check>()
                .Append<HeadingOrderCheck>()
                .Append<H1DuplicatesTitleCheck>()
                .Append<MissingLangCheck>()
                .Append<MissingViewportCheck>()
                .Append<DuplicateTitleCheck>()
                .Append<DuplicateDescriptionCheck>()
                .Append<DuplicateH1Check>()

                // Content.
                .Append<ThinContentCheck>()
                .Append<TextToHtmlRatioCheck>()
                .Append<PlaceholderContentCheck>()
                .Append<DuplicateContentCheck>()

                // Links.
                .Append<BrokenInternalLinkCheck>()
                .Append<BrokenExternalLinkCheck>()
                .Append<EmptyAnchorTextCheck>()
                .Append<GenericAnchorTextCheck>()
                .Append<TooManyLinksCheck>()
                .Append<DeepPageCheck>()

                // Images.
                .Append<BrokenImageCheck>()
                .Append<ImageAltCheck>()
                .Append<ImageDimensionsCheck>()
                .Append<ImageFormatCheck>()
                .Append<FaviconCheck>()

                // Structured data and social.
                .Append<InvalidJsonLdCheck>()
                .Append<OpenGraphCheck>()
                .Append<TwitterCardCheck>()

                // International.
                .Append<HreflangCodeCheck>()
                .Append<HreflangXDefaultCheck>();

            // The old ISiteCheck collection stays registered so an add-on that still contributes
            // to it keeps working - LegacySiteCheckAdapter wraps whatever is in it. The checks
            // this package used to put there have all been ported above.
            builder.WithCollectionBuilder<SiteAuditCheckCollectionBuilder>();

            builder.Components().Append<EnableModuleComponent>();
        }
    }
}
