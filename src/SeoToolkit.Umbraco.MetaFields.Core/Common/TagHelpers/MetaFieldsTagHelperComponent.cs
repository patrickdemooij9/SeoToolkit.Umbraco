using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers
{
    public class MetaFieldsTagHelperComponent : TagHelperComponent
    {
        private readonly IMetaFieldsService _seoService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public MetaFieldsTagHelperComponent(IMetaFieldsService seoService, IUmbracoContextFactory umbracoContextFactory)
        {
            _seoService = seoService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.Equals(context.TagName, "meta-fields", StringComparison.InvariantCultureIgnoreCase)) return Task.CompletedTask;

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            output.TagName = null;

            var stringBuilder = new StringBuilder();
            var metaTags = _seoService.Get(ctx.UmbracoContext.PublishedRequest.PublishedContent);
            if (metaTags is null)
                return Task.CompletedTask;
            foreach (var (key, value) in metaTags.Fields)
            {
                //TODO: We should probably have a special IsEmpty check here?
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                    continue;
                stringBuilder.AppendLine(key.Render(value).ToString());
            }

            output.PreContent.SetHtmlContent(new HtmlString(stringBuilder.ToString()));
            return Task.CompletedTask;

        }
    }
}
