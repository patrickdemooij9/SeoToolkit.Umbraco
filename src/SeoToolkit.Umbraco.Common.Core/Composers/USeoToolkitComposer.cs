using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.ContentApps;
using SeoToolkit.Umbraco.Common.Core.Dashboards;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using SeoToolkit.Umbraco.Common.Core.Sections;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;

namespace SeoToolkit.Umbraco.Common.Core.Composers
{
    public class SeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<SeoToolkitSection>();

            builder.Dashboards().Add<WelcomeDashboard>();

            builder.ContentApps().Append<SeoSettingsContentAppFactory>();
            builder.ContentApps().Append<SeoContentAppFactory>();

            builder.Services.AddSingleton<ModuleCollection>();

            builder.Services.AddUnique<ISeoSettingsRepository, SeoSettingsRepository>();
            builder.Services.AddUnique<ISeoSettingsService, SeoSettingsService>();
        }
    }
}
