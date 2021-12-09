using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.ScriptManager.Core.Enums;
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
            if (!config.ContainsKey("code") || string.IsNullOrWhiteSpace(config["code"].ToString()))
                return;

            model.AddScript(ScriptPositionType.HeadBottom, new HtmlString("<!-- Google Tag Manager -->" +
                "<script> (function(w, d, s, l, i){ w[l] = w[l] ||[]; w[l].push({" +
                    "'gtm.start':" +
                    "new Date().getTime(),event:'gtm.js'});var f = d.getElementsByTagName(s)[0]," +
                "j = d.createElement(s), dl = l != 'dataLayer' ? '&l=' + l : ''; j.async=true;j.src=" +
                "'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j, f);" +
            "})(window, document,'script','dataLayer','GTM-TJJ23Q9');</script>" +
                "<!-- End Google Tag Manager -->"));

            model.AddScript(ScriptPositionType.BodyTop, new HtmlString("<!-- Google Tag Manager (noscript) -->" +
                "<noscript><iframe src=\"https://www.googletagmanager.com/ns.html?id=GTM-TJJ23Q9\"" +
            "height=\"0\" width=\"0\" style=\"display:none;visibility:hidden\"></iframe></noscript>" +
                "<!--End Google Tag Manager(noscript)-->"));
        }
    }
}
