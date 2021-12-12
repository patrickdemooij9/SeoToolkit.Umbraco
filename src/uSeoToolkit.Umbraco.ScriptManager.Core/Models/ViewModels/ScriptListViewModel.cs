using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Models.ViewModels
{
    public class ScriptListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DefinitionName { get; set; }

        public ScriptListViewModel(Script script)
        {
            Id = script.Id;
            Name = script.Name;
            DefinitionName = script.Definition.Name;
        }
    }
}
