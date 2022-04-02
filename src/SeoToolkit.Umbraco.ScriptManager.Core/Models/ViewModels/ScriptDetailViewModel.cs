using System.Collections.Generic;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
{
    public class ScriptDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DefinitionAlias { get; set; }
        public Dictionary<string, object> Config { get; set; }

        public ScriptDetailViewModel(Script script)
        {
            Id = script.Id;
            Name = script.Name;
            DefinitionAlias = script.Definition.Alias;
            Config = script.Config;
        }
    }
}
