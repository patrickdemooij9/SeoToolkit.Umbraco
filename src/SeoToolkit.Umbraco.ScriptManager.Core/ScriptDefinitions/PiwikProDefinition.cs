using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.PropertyEditors;

namespace SeoToolkit.Umbraco.ScriptManager.Core.ScriptDefinitions
{
    public class PiwikProDefinition : IScriptDefinition
    {
        private readonly ViewRenderHelper _viewRenderHelper;
        private const string accountAddressKey = "accountAddress";
        private const string siteIdKey = "siteId";

        //Name of the definition
        public string Name => "Piwik PRO";
        //Alias of the definition. Used in the database
        public string Alias => "piwikpro";

        //Umbraco Configuration fields of the definition
        public ConfigurationField[] Fields => new ConfigurationField[]
        {
            new () {
                Key = accountAddressKey,
                Name = "Account Address",
                Description = "Your Piwik PRO account address. Make sure the address contains https:// and a trailing slash.",
                View = "textstring"
            },
            new () {
                Key = siteIdKey,
                Name = "Site Id",
                Description = "Your Piwik PRO site ID",
                View = "textstring"
            }
        };

        public PiwikProDefinition(ViewRenderHelper viewRenderHelper)
        {
            _viewRenderHelper = viewRenderHelper;
        }

        //Render function where you add your script to the correct location. You can use the ViewRenderHelper to render a cshtml view.
        public void Render(ScriptRenderModel model, Dictionary<string, object> config)
        {
            if (!config.ContainsKey(accountAddressKey) || string.IsNullOrWhiteSpace(config[accountAddressKey]?.ToString()))
                return;

            if (!config.ContainsKey(siteIdKey) || string.IsNullOrWhiteSpace(config[siteIdKey]?.ToString()))
                return;

            var accountAddress = config[accountAddressKey].ToString();
            var siteId = config[siteIdKey].ToString();

            string[] viewData = new[] { accountAddress, siteId };

            model.AddScript(ScriptPositionType.BodyTop, _viewRenderHelper.RenderView("~/Views/ScriptManager/PiwikPro/Script.cshtml", viewData));
        }
    }
}
