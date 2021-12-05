using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Collections
{
    public class ScriptDefinitionCollection : BuilderCollectionBase<IScriptDefinition>
    {
        public ScriptDefinitionCollection(Func<IEnumerable<IScriptDefinition>> items) : base(items)
        {
        }
    }
}
