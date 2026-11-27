using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.DeliveryApi;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Repositories.Domains;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Common.Core.Startup;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Composers
{
    public class SeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Global");
            builder.Services.Configure<GlobalAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<GlobalConfig>), typeof(GlobalConfigService));

            var settings = section?.Get<GlobalAppSettingsModel>();

            builder.AddBackOfficeOpenApiDocument("seoToolkit", document => document.WithTitle("SeoToolkit"));

            // Pinned JSON options for the SeoToolkit backoffice controllers (see SeoToolkitAuthenticatedControllerBase)
            builder.Services.AddControllers().AddJsonOptions(JsonOptionsConstants.SeoToolkit, options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddSingleton<ModuleCollection>();

            builder.Services.AddUnique<ISeoSettingsRepository, SeoSettingsRepository>();
            builder.Services.AddUnique<ISeoSettingsService, SeoSettingsService>();

            builder.Services.AddSingleton<SeoKeyValueRepository>();
            builder.Services.AddUnique<ISeoKeyValueRepository>(serviceProvider => new CachedSeoKeyValueRepository(
                serviceProvider.GetRequiredService<SeoKeyValueRepository>(),
                serviceProvider.GetRequiredService<AppCaches>(),
                serviceProvider.GetRequiredService<DistributedCache>()));
            builder.Services.AddUnique<ISeoKeyValueService, SeoKeyValueService>();

            builder.Services.AddUnique<ISeoDomainsRepository, SeoDomainsRepository>();
            builder.Services.AddUnique<ISeoDomainsService, SeoDomainsService>();
            builder.Services.AddUnique<ISeoDomainResolver, SeoDomainResolver>();

            if (settings?.EnableDeliveryApiSupport is true)
            {
                builder.Services.AddSingleton<IApiContentResponseBuilder, SeoToolkitResponseBuilder>();
            }

            builder.WithCollectionBuilder<SeoTreeSectionCollectionBuilder>()
                .Add<SeoToolkitInfoSection>();

            builder.WithCollectionBuilder<SeoKeyValueSettingCollectionBuilder>();

            builder.WithCollectionBuilder<DashboardDataSourceCollectionBuilder>();
        }
    }
}
