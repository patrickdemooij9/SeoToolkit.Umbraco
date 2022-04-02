using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Collections
{
    public class ScriptDefinitionCollectionBuilder : WeightedCollectionBuilderBase<ScriptDefinitionCollectionBuilder, ScriptDefinitionCollection, IScriptDefinition>
    {
        protected override ScriptDefinitionCollectionBuilder This => this;
    }
}
