using System.Collections.Generic;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Business
{
    public class Script
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IScriptDefinition Definition { get; set; }
        public int[] DocumentTypeIds { get; set; }
        public Dictionary<string, string> Config { get; set; }
    }
}
