using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Collections
{
    public class ScriptDefinitionCollectionBuilder : WeightedCollectionBuilderBase<ScriptDefinitionCollectionBuilder, ScriptDefinitionCollection, IScriptDefinition>
    {
        protected override ScriptDefinitionCollectionBuilder This => this;
    }
}
