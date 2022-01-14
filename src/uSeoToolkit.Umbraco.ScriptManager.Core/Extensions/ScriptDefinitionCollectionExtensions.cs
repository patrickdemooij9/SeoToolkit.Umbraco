using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.ScriptManager.Core.Collections;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Extensions
{
    public static class ScriptDefinitionCollectionExtensions
    {
        public static ScriptDefinitionCollectionBuilder ScriptDefinitions(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ScriptDefinitionCollectionBuilder>();
    }
}
