using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;
using uSeoToolkit.Umbraco.ScriptManager.Core.Components;
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
                .Add<GoogleTagManagerDefinition>();

            builder.Components().Append<ScriptManagerDatabaseComponent>();

            builder.Services.AddScoped<IScriptRepository, ScriptRepository>();
            builder.Services.AddScoped<IScriptManagerService, ScriptManagerService>();
        }
    }
}
