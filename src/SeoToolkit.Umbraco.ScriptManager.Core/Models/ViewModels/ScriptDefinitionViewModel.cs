using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
{
    public class ScriptDefinitionViewModel
    {
        public string Name { get; }
        public string Alias { get; }
        public ScriptField[] Fields { get; }

        public ScriptDefinitionViewModel(IScriptDefinition scriptDefinition)
        {
            Name = scriptDefinition.Name;
            Alias = scriptDefinition.Alias;
            Fields = scriptDefinition.Fields;
        }
    }
}
