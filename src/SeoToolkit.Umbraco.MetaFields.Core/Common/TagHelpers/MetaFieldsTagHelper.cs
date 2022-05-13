using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers
{
    [HtmlTargetElement("meta-fields")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetaFieldsTagHelper : TagHelperComponentTagHelper
    {
        public MetaFieldsTagHelper(ITagHelperComponentManager manager, ILoggerFactory loggerFactory) : base(manager, loggerFactory)
        {
        }
    }
}
