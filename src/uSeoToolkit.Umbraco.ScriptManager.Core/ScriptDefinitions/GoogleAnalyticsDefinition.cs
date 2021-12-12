using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Enums;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleAnalyticsDefinition : IScriptDefinition
    {
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
        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(PropertyIdKey) || string.IsNullOrWhiteSpace(config[PropertyIdKey]?.ToString()))
                return;

            var propertyId = config[PropertyIdKey];
            model.AddScript(ScriptPositionType.HeadBottom, new HtmlString(
                "<!-- Google Analytics -->" + 
                "<script>" + 
                "(function(i, s, o, g, r, a, m){ i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function(){" + 
                "(i[r].q = i[r].q ||[]).push(arguments)},i[r].l = 1 * new Date(); a = s.createElement(o)," + 
                "m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)" + 
                "})(window, document,'script','https://www.google-analytics.com/analytics.js','ga');" + 
                "ga('create', '" + propertyId + "', 'auto');" + 
                "ga('send', 'pageview');" +
                "</script>" +
                "<!-- End Google Analytics -->"));
        }
    }
}
