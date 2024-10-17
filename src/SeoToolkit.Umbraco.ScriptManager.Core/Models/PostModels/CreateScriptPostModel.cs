using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.PostModels
{
    public class CreateScriptPostModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DefinitionAlias { get; set; }
        public Dictionary<string, string> Fields { get; set; }
    }
}
