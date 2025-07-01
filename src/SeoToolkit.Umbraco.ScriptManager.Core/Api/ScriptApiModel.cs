using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Api
{
    public class ScriptApiModel
    {
        public string DefinitionAlias { get; set; }
        public Dictionary<string, object> Config { get; set; }
    }
}
