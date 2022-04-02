using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers
{
    public class MetaFieldsTagHelper : TagHelper
    {
        private readonly IMetaFieldsService _seoService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public MetaFieldsTagHelper(IMetaFieldsService seoService, IUmbracoContextFactory umbracoContextFactory)
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
                //TODO: We should probably have a special IsEmpty check here?
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                    continue;
                stringBuilder.AppendLine(key.Render(value).ToString());
            }

            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
        }
    }
}
