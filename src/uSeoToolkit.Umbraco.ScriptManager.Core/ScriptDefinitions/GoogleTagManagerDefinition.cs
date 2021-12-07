using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleTagManagerDefinition : IScriptDefinition
    {
        private readonly IIOHelper _ioHelper;
        public string Name => "Google Tag Manager";
        public string Alias => "googleTagManager";

        public GoogleTagManagerDefinition(IIOHelper ioHelper)
        {
            _ioHelper = ioHelper;
        }

        public ConfigurationField[] Fields => new[]
        {
            new ConfigurationField
            {
                Key = "code",
                Name = "Code",
                Description = "Code that is used for your GTM instance",
                View = "textstring"
            }
        };

        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            throw new NotImplementedException();
        }
    }
}
