using System;
using System.Linq;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditors;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldEditors;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField
{
    [Weight(500)]
    public class TwitterImageField : SeoField<IPublishedContent>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        public override string Title => "Twitter Image";
        public override string Alias => SeoFieldAliasConstants.TwitterImage;
        public override string Description => "Image for Twitter";
        public override string GroupAlias => SeoFieldGroupConstants.TwitterGroup;
      

        public override ISeoFieldEditor Editor => new SeoFieldFieldsEditor(new[] { "Umbraco.MediaPicker", "Umbraco.MediaPicker3" });
        public override ISeoFieldEditEditor EditEditor => new SeoImageEditEditor(_umbracoContextFactory);

        public TwitterImageField(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
       
        
        }
     

        protected override HtmlString Render(IPublishedContent value)
        {
            string url;
      
            if (value is { } media)
            {
                url = media.Url(mode: UrlMode.Absolute);
           
            }
            else
            {
                url = value?.ToString();
            }

            var html = $"<meta name=\"twitter:image\" content=\"{url}\"/>";
      
            

            return new HtmlString(html);
        }
    }
}