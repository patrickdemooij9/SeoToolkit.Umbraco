using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Business
{
    public class Script
    {
        [Obsolete("Use Key property instead")]
        public int Id { get; set; }
        public Guid? Key {get; set;}
        public string Name { get; set; }
        public IScriptDefinition Definition { get; set; }
        public Dictionary<string, string> Config { get; set; }
        public Guid? DomainId { get; set; }
        public int SortOrder { get; set; }
    }
}
