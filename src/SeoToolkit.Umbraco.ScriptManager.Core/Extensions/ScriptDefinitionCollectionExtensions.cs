using Umbraco.Cms.Core.DependencyInjection;
using SeoToolkit.Umbraco.ScriptManager.Core.Collections;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Extensions
{
    public static class ScriptDefinitionCollectionExtensions
    {
        public static ScriptDefinitionCollectionBuilder ScriptDefinitions(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ScriptDefinitionCollectionBuilder>();
    }
}
