using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Startup;
using System;
using System.Text;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.ScriptManager.Core.TagHelpers
{
    public class RenderScriptTagHelper : TagHelper
    {
        private readonly IScriptManagerService _scriptManagerService;
        private readonly ISeoDomainResolver _seoDomainResolver;

        public ScriptPositionType Position { get; set; }

        public RenderScriptTagHelper(IScriptManagerService scriptManagerService, ISeoDomainResolver seoDomainResolver)
        {
            _scriptManagerService = scriptManagerService;
            _seoDomainResolver = seoDomainResolver;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var stringBuilder = new StringBuilder();
            var domain = _seoDomainResolver.ResolveDomain();
            if (domain != null && !domain.HasFunctionality($"Module.{ScriptManagerTreeSection.SectionGuid}"))
            {
                domain = null;
            }

            foreach (var script in _scriptManagerService.GetRender(domain?.Id).Get(Position))
            {
                stringBuilder.Append(script.ToHtmlString());
            }

            output.TagName = null;
            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
        }
    }
}
