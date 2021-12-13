using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;
using uSeoToolkit.Umbraco.ScriptManager.Core.Components;
using uSeoToolkit.Umbraco.ScriptManager.Core.Config;
using uSeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.ScriptManager.Core.Repositories;
using uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions;
using uSeoToolkit.Umbraco.ScriptManager.Core.Services;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Composers
{
    public class ScriptManagerComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.WithCollectionBuilder<ScriptDefinitionCollectionBuilder>()
                .Add<GoogleTagManagerDefinition>()
                .Add<GoogleAnalyticsDefinition>()
                .Add<HotjarDefinition>()
                .Add<CustomScriptDefinition>();

            builder.Components().Append<ScriptManagerDatabaseComponent>();

            builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
            builder.Services.AddScoped<IScriptManagerService, ScriptManagerService>();
            builder.Services.AddSingleton(typeof(ISettingsService<ScriptManagerConfigModel>), typeof(ScriptManagerConfigurationService));

            builder.Services.Configure<ScriptManagerAppSettingsModel>(builder.Config.GetSection("uSeoToolkit:ScriptManager"));
        }
    }
}
