using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class CustomScriptDefinition : IScriptDefinition
    {
        private const string ScriptBottomHeadKey = "scriptBottomHead";
        private const string ScriptTopBodyKey = "scriptTopBody";
        private const string ScriptBottomBodyKey = "scriptBottomBody";

        public string Name => "Custom Script";
        public string Alias => "customScriptDefinition";
        public ScriptField[] Fields =>
        [
            new() {
                Key = ScriptBottomHeadKey,
                Name = "Script Bottom Head",
                Description = "Script that renders at the bottom of the head",
                PropertyAlias = "Umb.PropertyEditorUi.TextArea",
            },
            new() {
                Key = ScriptTopBodyKey,
                Name = "Script Top Body",
                Description = "Script that renders at the top of the body",
                PropertyAlias = "Umb.PropertyEditorUi.TextArea"
            },
            new() {
                Key = ScriptBottomBodyKey,
                Name = "Script Bottom Body",
                Description = "Script that renders at the bottom of the body",
                PropertyAlias = "Umb.PropertyEditorUi.TextArea"
            } 
        ];

        public void Render(ScriptRenderModel model, Dictionary<string, string> config)
        {
            if (config.ContainsKey(ScriptBottomHeadKey) && !string.IsNullOrWhiteSpace(config[ScriptBottomHeadKey]?.ToString()))
                model.AddScript(ScriptPositionType.HeadBottom, new HtmlString(config[ScriptBottomHeadKey].ToString()));

            if (config.ContainsKey(ScriptTopBodyKey) && !string.IsNullOrWhiteSpace(config[ScriptTopBodyKey]?.ToString()))
                model.AddScript(ScriptPositionType.BodyTop, new HtmlString(config[ScriptTopBodyKey].ToString()));

            if (config.ContainsKey(ScriptBottomBodyKey) && !string.IsNullOrWhiteSpace(config[ScriptBottomBodyKey]?.ToString()))
                model.AddScript(ScriptPositionType.BodyBottom, new HtmlString(config[ScriptBottomBodyKey].ToString()));
        }
    }
}
