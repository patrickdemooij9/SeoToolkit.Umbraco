using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IScriptDefinition
    {
        string Name { get; }
        string Alias { get; }
        ConfigurationField[] Fields { get; }

        void Render(ScriptRenderModel model, Dictionary<string, object> config);
    }
}
