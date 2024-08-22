using System;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(500)]
    public class OpenGraphVideoField : SeoField<IPublishedContent>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        public override string Title => "Open Graph Video";
        public override string Alias => SeoFieldAliasConstants.OpenGraphVideo;
        public override string Description => "Video for Open Graph";
        public override string GroupAlias => SeoFieldGroupConstants.OpenGraphGroup;
       

        public override ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.MediaPicker3" });
        public override ISeoFieldEditEditor EditEditor => new SeoVideoEditEditor(_umbracoContextFactory);

        public OpenGraphVideoField(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        protected override HtmlString Render(IPublishedContent value)
        {
            string url;
            string height = string.Empty;
            string width = string.Empty;
            string type = string.Empty;
            if (value is IPublishedContent media)
            {
                url = media.Url(mode: UrlMode.Absolute);
                width = media.Value<string>("umbracoWidth");
                height = media.Value<string>("umbracoHeight");
                type = media.Value<string>("umbracoExtension");
            }
            else
            {
                url = value?.ToString();
            }

            var html = $"<meta property=\"og:video\" content=\"{url}\"/>";
            if (!string.IsNullOrEmpty(width))
            {
                html += $"<meta property=\"og:video:width\" content=\"{width}\"/>";
            }
            if (!string.IsNullOrEmpty(height))
            {
                html += $"<meta property=\"og:video:height\" content=\"{height}\"/>";
            }
            if (!string.IsNullOrEmpty(type))
            {
                html += $"<meta property=\"og:video:type\" content=\"{type}\"/>";
            }
            

            return new HtmlString(html);
        }
    }
}