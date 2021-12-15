using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Enums;
using uSeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleAnalyticsDefinition : IScriptDefinition
    {
        private readonly ViewRenderHelper _viewRenderHelper;
        private const string PropertyIdKey = "propertyId";

        public string Name => "Google Analytics";
        public string Alias => "googleAnalytics";
        public ConfigurationField[] Fields => new ConfigurationField[]
        {
            new ConfigurationField
            {
                Key = PropertyIdKey,
                Name = "Property Id",
                Description = "Property id for Google Analytics",
                View = "textstring"
            },
        };

        public GoogleAnalyticsDefinition(ViewRenderHelper viewRenderHelper)
        {
            _viewRenderHelper = viewRenderHelper;
        }

        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(PropertyIdKey) || string.IsNullOrWhiteSpace(config[PropertyIdKey]?.ToString()))
                return;

            var propertyId = config[PropertyIdKey].ToString();
            model.AddScript(ScriptPositionType.HeadBottom, _viewRenderHelper.RenderView("~/Views/ScriptManager/GoogleAnalytics/Script.cshtml", propertyId));
        }
    }
}
