using Microsoft.AspNetCore.Html;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Api
{
    public class ScriptRenderApiModel
    {
        public HtmlString[] HeadBottom { get; set; }
        public HtmlString[] BodyTop { get; set; }
        public HtmlString[] BodyBottom { get; set; }
    }
}
