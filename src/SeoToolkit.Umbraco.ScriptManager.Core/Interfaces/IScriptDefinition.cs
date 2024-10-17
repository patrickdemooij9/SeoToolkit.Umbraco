using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IScriptDefinition
    {
        string Name { get; }
        string Alias { get; }
        ScriptField[] Fields { get; }

        void Render(ScriptRenderModel model, Dictionary<string, object> config);
    }
}
