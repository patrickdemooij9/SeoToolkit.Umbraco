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

            var publishedContent = ctx.UmbracoContext.PublishedRequest?.PublishedContent;
            if (publishedContent is null)
                return;

            var stringBuilder = new StringBuilder();
            var metaTags = _seoService.Get(publishedContent, true);
            if (metaTags is null)
                return;
            foreach (var (key, value) in metaTags.Fields)
            {
                //TODO: We should probably have a special IsEmpty check here?
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                    continue;
                var renderedValue = key.Render(value);
                if (renderedValue is null)
                    continue;

                stringBuilder.AppendLine(renderedValue.ToString());
            }

            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
        }
    }
}
