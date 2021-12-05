using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business
{
    public class Script
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IScriptDefinition Definition { get; set; }
        public int[] DocumentTypeIds { get; set; }
        public ScriptConfig Config { get; set; }
    }
}
