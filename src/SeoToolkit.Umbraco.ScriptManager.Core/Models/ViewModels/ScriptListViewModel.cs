using System;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
{
    public class ScriptListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DefinitionName { get; set; }

        public ScriptListViewModel(Script script)
        {
            Id = script.Key.Value;
            Name = script.Name;
            DefinitionName = script.Definition.Name;
        }
    }
}
