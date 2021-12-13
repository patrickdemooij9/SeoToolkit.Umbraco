using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Config.Models
{
    public class ScriptManagerAppSettingsModel
    {
        public Dictionary<string, ScriptManagerDefinitionAppSettingsModel> Definitions { get; set; }
            = new Dictionary<string, ScriptManagerDefinitionAppSettingsModel>();
    }
}
