using System;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleTagManagerDefinition : IScriptDefinition
    {
        public string Name => "Google Tag Manager";
        public string Alias => "googleTagManager";

        public ConfigurationField[] Fields => new[]
        {
            new ConfigurationField
            {
                Name = "Code",
                Description = "Code that is used for your GTM instance",
                View = "textstring"
            }
        };
        public void Render(ScriptRenderModel model)
        {
            throw new NotImplementedException();
        }
    }
}
