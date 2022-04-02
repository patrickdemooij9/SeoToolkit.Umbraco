using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Config.Models
{
    public class ScriptManagerAppSettingsModel
    {
        public Dictionary<string, ScriptManagerDefinitionAppSettingsModel> Definitions { get; set; }
            = new Dictionary<string, ScriptManagerDefinitionAppSettingsModel>();

        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}
