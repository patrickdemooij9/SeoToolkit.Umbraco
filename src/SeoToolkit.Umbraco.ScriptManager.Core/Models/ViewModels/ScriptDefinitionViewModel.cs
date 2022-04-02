using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
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
