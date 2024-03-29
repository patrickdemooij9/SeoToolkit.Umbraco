﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.PropertyEditors;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class HotjarDefinition : IScriptDefinition
    {
        private readonly ViewRenderHelper _viewRenderHelper;
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

        public HotjarDefinition(ViewRenderHelper viewRenderHelper)
        {
            _viewRenderHelper = viewRenderHelper;
        }

        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(TrackingIdProperty) ||
                string.IsNullOrWhiteSpace(config[TrackingIdProperty]?.ToString()))
                return;

            var trackingId = config[TrackingIdProperty].ToString();
            model.AddScript(ScriptPositionType.HeadBottom,
                _viewRenderHelper.RenderView("~/Views/ScriptManager/Hotjar/Script.cshtml", trackingId));
        }
    }
}
