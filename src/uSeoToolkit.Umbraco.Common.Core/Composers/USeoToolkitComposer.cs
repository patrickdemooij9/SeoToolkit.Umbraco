using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Collections;
using uSeoToolkit.Umbraco.Common.Core.Components;
using uSeoToolkit.Umbraco.Common.Core.ContentApps;
using uSeoToolkit.Umbraco.Common.Core.Dashboards;
using uSeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;
using uSeoToolkit.Umbraco.Common.Core.Sections;
using uSeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;

namespace uSeoToolkit.Umbraco.Common.Core.Composers
{
    public class USeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<USeoToolkitSection>();
            builder.Dashboards().Add<WelcomeDashboard>();
            builder.ContentApps().Append<SeoSettingsContentAppFactory>();

            builder.Components().Append<SeoSettingsDatabaseComponent>();

            builder.Services.AddSingleton<ModuleCollection>();

            builder.Services.AddUnique<ISeoSettingsRepository, SeoSettingsRepository>();
            builder.Services.AddUnique<ISeoSettingsService, SeoSettingsService>();
        }
    }
}
