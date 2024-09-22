using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Swagger;

namespace SeoToolkit.Umbraco.Common.Core.Composers
{
    public class SeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Global");
            builder.Services.Configure<GlobalAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<GlobalConfig>), typeof(GlobalConfigService));

            builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

            //builder.ContentApps().Append<SeoSettingsContentAppFactory>();
            //builder.ContentApps().Append<SeoContentAppFactory>();

            builder.Services.AddSingleton<ModuleCollection>();

            builder.Services.AddUnique<ISeoSettingsRepository, SeoSettingsRepository>();
            builder.Services.AddUnique<ISeoSettingsService, SeoSettingsService>();
        }
    }
}
