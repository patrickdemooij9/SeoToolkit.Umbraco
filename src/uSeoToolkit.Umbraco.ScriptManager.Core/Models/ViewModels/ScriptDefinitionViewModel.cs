using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
{
    public class ScriptDefinitionViewModel
    {
        public string Name { get; }
        public string Alias { get; }
        public ConfigurationField[] Fields { get; }

        public ScriptDefinitionViewModel(IScriptDefinition scriptDefinition)
        {
            Name = scriptDefinition.Name;
            Alias = scriptDefinition.Alias;
            Fields = scriptDefinition.Fields;
        }
    }
}
