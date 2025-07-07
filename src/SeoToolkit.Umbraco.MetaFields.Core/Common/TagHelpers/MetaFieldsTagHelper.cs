using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers
{
    public class MetaFieldsTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

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
            var content = ctx.UmbracoContext.PublishedRequest.PublishedContent;
            if (content is null)
            {
                // Fix for https://github.com/umbraco/Umbraco-CMS/issues/12834
                content = ViewContext.HttpContext.Features.Get<UmbracoRouteValues>().PublishedRequest.PublishedContent;
            }
            var metaTags = _seoService.Get(content, true);
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
