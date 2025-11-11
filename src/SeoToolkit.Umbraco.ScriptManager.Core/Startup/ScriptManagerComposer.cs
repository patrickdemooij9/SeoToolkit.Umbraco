using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Api;
using SeoToolkit.Umbraco.ScriptManager.Core.Components;
using SeoToolkit.Umbraco.ScriptManager.Core.Config;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Extensions;
using SeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Repositories;
using SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions;
using SeoToolkit.Umbraco.ScriptManager.Core.Services;
using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using SeoToolkit.Umbraco.ScriptManager.Core.Startup;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Composers
{
    public class ScriptManagerComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:ScriptManager");
            builder.Services.Configure<ScriptManagerAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<ScriptManagerConfigModel>), typeof(ScriptManagerConfigurationService));

            var disabledModules = section?.Get<ScriptManagerAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();
            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                return;
            }

            if (!disabledModules.Contains(DisabledModuleConstant.SectionTree))
            {
                builder.WithCollectionBuilder<SeoTreeSectionCollectionBuilder>()
                    .Add<ScriptManagerTreeSection>();
            }

            if (!disabledModules.Contains(DisabledModuleConstant.Api))
            {
                builder.Services.AddScoped<IApiDataHandler, ScriptManagerApiHandler>();
            }

            builder.ScriptDefinitions()
                .Add<GoogleTagManagerDefinition>()
                .Add<GoogleAnalyticsDefinition>()
                .Add<HotjarDefinition>()
                .Add<CustomScriptDefinition>()
                .Add<PiwikProDefinition>();

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
            builder.Services.AddScoped<IScriptManagerService, ScriptManagerService>();
            builder.Services.AddSingleton<ViewRenderHelper>();
        }
    }
}
