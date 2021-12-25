using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Web;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers
{
    public class MetaFieldsTagHelper : TagHelper
    {
        private readonly ISeoService _seoService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public MetaFieldsTagHelper(ISeoService seoService, IUmbracoContextFactory umbracoContextFactory)
        {
            _seoService = seoService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            output.TagName = null;

            var stringBuilder = new StringBuilder();
            var metaTags = _seoService.Get(ctx.UmbracoContext.PublishedRequest.PublishedContent);
            if (metaTags is null)
                return;
            foreach (var (key, value) in metaTags.Fields)
            {
                stringBuilder.AppendLine(key.Render(value).ToString());
            }

            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
        }
    }
}
