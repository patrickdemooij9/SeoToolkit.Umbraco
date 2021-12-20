using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using uSeoToolkit.Umbraco.MetaFields.Core.Constants;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(500)]
    public class OpenGraphImageField : ISeoField
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        public string Title => "Open Graph Image";
        public string Alias => SeoFieldAliasConstants.OpenGraphImage;
        public string Description => "Image for Open Graph";
        public Type FieldType => typeof(string);

        public ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.MediaPicker" });
        public ISeoFieldEditEditor EditEditor => new SeoImageEditEditor(_umbracoContextFactory);

        public OpenGraphImageField(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public HtmlString Render(object value)
        {
            string url;
            if (value is IPublishedContent media)
            {
                url = media.Url(mode: UrlMode.Absolute);
            }
            else
            {
                url = value?.ToString();
            }

            return new HtmlString($"<meta property='og:image' content='{url}'/>");
        }
    }
}
