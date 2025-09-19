using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Api
{
    public class ScriptApiModel
    {
        public string DefinitionAlias { get; set; }
        public Dictionary<string, string> Config { get; set; }
    }
}
