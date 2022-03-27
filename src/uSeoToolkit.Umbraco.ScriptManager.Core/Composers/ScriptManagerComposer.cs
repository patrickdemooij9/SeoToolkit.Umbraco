using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Constants;
using uSeoToolkit.Umbraco.Common.Core.Extensions;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.ScriptManager.Core.Components;
using uSeoToolkit.Umbraco.ScriptManager.Core.Config;
using uSeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using uSeoToolkit.Umbraco.ScriptManager.Core.Controllers;
using uSeoToolkit.Umbraco.ScriptManager.Core.Extensions;
using uSeoToolkit.Umbraco.ScriptManager.Core.Helpers;
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
            var section = builder.Config.GetSection("uSeoToolkit:ScriptManager");
            builder.Services.Configure<ScriptManagerAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<ScriptManagerConfigModel>), typeof(ScriptManagerConfigurationService));

            var disabledModules = section?.Get<ScriptManagerAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                builder.Trees().RemoveTreeController(typeof(ScriptManagerTreeController));
                return;
            }

            builder.ScriptDefinitions()
                .Add<GoogleTagManagerDefinition>()
                .Add<GoogleAnalyticsDefinition>()
                .Add<HotjarDefinition>()
                .Add<CustomScriptDefinition>();

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
            builder.Services.AddScoped<IScriptManagerService, ScriptManagerService>();
            builder.Services.AddSingleton<ViewRenderHelper>();
        }
    }
}
