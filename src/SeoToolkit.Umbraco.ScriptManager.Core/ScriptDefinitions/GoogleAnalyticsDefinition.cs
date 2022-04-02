using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleAnalyticsDefinition : IScriptDefinition
    {
        private readonly ViewRenderHelper _viewRenderHelper;
        private const string PropertyIdKey = "propertyId";

        //Name of the definition
        public string Name => "Google Analytics";
        //Alias of the definition. Used in the database
        public string Alias => "googleAnalytics";

        //Umbraco Configuration fields of the definition
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

        //Render function where you add your script to the correct location. You can use the ViewRenderHelper to render a cshtml view.
        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(PropertyIdKey) || string.IsNullOrWhiteSpace(config[PropertyIdKey]?.ToString()))
                return;

            var propertyId = config[PropertyIdKey].ToString();
            model.AddScript(ScriptPositionType.HeadBottom, _viewRenderHelper.RenderView("~/Views/ScriptManager/GoogleAnalytics/Script.cshtml", propertyId));
        }
    }
}
