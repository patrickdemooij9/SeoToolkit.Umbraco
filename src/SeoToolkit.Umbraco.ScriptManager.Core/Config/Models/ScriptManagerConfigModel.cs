using System;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Config.Models
{
    public class ScriptManagerConfigModel
    {
        public ScriptManagerDefinitionConfigModel[] Definitions { get; set; } =
            Array.Empty<ScriptManagerDefinitionConfigModel>();

        public bool RenderScriptsInApi { get; set; }

        public string[] DisabledModules { get; set; } = Array.Empty<string>();
        public bool DisableRenderCaching { get; set; } = false;
    }
}
