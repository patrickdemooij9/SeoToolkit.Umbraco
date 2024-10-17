using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class GoogleTagManagerDefinition : IScriptDefinition
    {
        private readonly ViewRenderHelper _viewRenderHelper;
        public string Name => "Google Tag Manager";
        public string Alias => "googleTagManager";

        public ScriptField[] Fields => new[]
        {
            new ScriptField
            {
                Key = "code",
                Name = "Code",
                Description = "Code that is used for your GTM instance",
                PropertyAlias = "Umb.PropertyEditorUi.TextBox"
            }
        };

        public GoogleTagManagerDefinition(ViewRenderHelper viewRenderHelper)
        {
            _viewRenderHelper = viewRenderHelper;
        }

        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey("code") || string.IsNullOrWhiteSpace(config["code"].ToString()))
                return;

            var code = config["code"].ToString();

            model.AddScript(ScriptPositionType.HeadBottom, _viewRenderHelper.RenderView("~/Views/ScriptManager/GTM/HeadBottom.cshtml", code));
            model.AddScript(ScriptPositionType.BodyTop, _viewRenderHelper.RenderView("~/Views/ScriptManager/GTM/BodyTop.cshtml", code));
        }
    }
}
