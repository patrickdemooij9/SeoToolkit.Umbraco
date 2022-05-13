using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace SeoToolkit.Umbraco.ScriptManager.Core.TagHelpers
{
    [HtmlTargetElement("render-script")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RenderScriptTagHelper : TagHelperComponentTagHelper
    {
        public RenderScriptTagHelper(ITagHelperComponentManager manager, ILoggerFactory loggerFactory) : base(manager, loggerFactory)
        {
        }
    }
}
