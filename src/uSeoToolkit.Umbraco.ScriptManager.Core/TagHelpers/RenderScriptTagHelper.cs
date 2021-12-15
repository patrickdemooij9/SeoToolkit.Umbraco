using System;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.ScriptManager.Core.Enums;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.TagHelpers
{
    public class RenderScriptTagHelper : TagHelper
    {
        private readonly IScriptManagerService _scriptManagerService;
        public ScriptPositionType Position { get; set; }

        public RenderScriptTagHelper(IScriptManagerService scriptManagerService)
        {
            _scriptManagerService = scriptManagerService;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var stringBuilder = new StringBuilder();
            foreach (var script in _scriptManagerService.GetRender().Get(Position))
            {
                stringBuilder.Append(script.ToHtmlString());
            }

            output.TagName = null;
            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
        }
    }
}
