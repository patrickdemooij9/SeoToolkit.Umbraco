using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Enums;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class HotjarDefinition : IScriptDefinition
    {
        private const string TrackingIdProperty = "trackingId";

        public string Name => "Hotjar";
        public string Alias => "hotjar";
        public ConfigurationField[] Fields => new ConfigurationField[]
        {
            new ConfigurationField
            {
                Key = TrackingIdProperty,
                Name = "Tracking Id",
                Description = "Tracking Id used by hotjar",
                View = "textstring"
            },
        };
        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(TrackingIdProperty) ||
                string.IsNullOrWhiteSpace(config[TrackingIdProperty]?.ToString()))
                return;

            var trackingId = config[TrackingIdProperty].ToString();
            model.AddScript(ScriptPositionType.HeadBottom, new HtmlString("<!-- Hotjar Tracking Code -->" +
                                                                          "<script>" +
                                                                          "(function(h, o, t, j, a, r){" +
                                                                          "h.hj = h.hj || function(){ (h.hj.q=h.hj.q ||[]).push(arguments)};" +
                                                                          "h._hjSettings={hjid:" + trackingId + ",hjsv:6};" +
                                                                          "a=o.getElementsByTagName('head')[0];" +
                                                                          "r=o.createElement('script'); r.async = 1;" +
                                                                          "r.src=t + h._hjSettings.hjid + j + h._hjSettings.hjsv;" +
                                                                          "a.appendChild(r);" +
                                                                          "})(window, document, 'https://static.hotjar.com/c/hotjar-', '.js?sv=');" +
                                                                          "</script>"));
        }
    }
}
