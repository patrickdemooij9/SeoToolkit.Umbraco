using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;

namespace SeoToolkit.Umbraco.ScriptManager.Core.TagHelpers
{
    public class RenderScriptTagHelperComponent : TagHelperComponent
    {
        private readonly IScriptManagerService _scriptManagerService;
        public ScriptPositionType Position { get; set; }

        public RenderScriptTagHelperComponent(IScriptManagerService scriptManagerService)
        {
            _scriptManagerService = scriptManagerService;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.Equals(context.TagName, "render-script", StringComparison.InvariantCultureIgnoreCase)) return Task.CompletedTask;

            var stringBuilder = new StringBuilder();
            foreach (var script in _scriptManagerService.GetRender().Get(Position))
            {
                stringBuilder.Append(script.ToHtmlString());
            }

            output.TagName = null;
            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
            return Task.CompletedTask;
        }
    }
}
