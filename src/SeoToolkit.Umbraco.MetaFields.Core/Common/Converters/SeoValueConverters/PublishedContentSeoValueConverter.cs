using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System.IO;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using System.Linq;
using static Umbraco.Cms.Core.Constants.HttpContext;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class PublishedContentSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(IPublishedContent);
        public Type ToValue => typeof(string);

        private ISettingsService<MetaFieldsConfigModel> _settingsService;

        public PublishedContentSeoValueConverter(ISettingsService<MetaFieldsConfigModel> settingsService)
        {
            _settingsService = settingsService;
        }

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (value is not IPublishedContent content) return null;

            var settings = _settingsService.GetSettings();
            if (content.ItemType == PublishedItemType.Media)
            {
                if (!settings.SupportedMediaTypes.Contains(Path.GetExtension(content.Url()), StringComparer.InvariantCultureIgnoreCase)) return null;

                if (string.IsNullOrWhiteSpace(settings.OpenGraphCropAlias)) return content.Url(mode: UrlMode.Absolute);

                var crops = content.Value<ImageCropperValue>("umbracoFile");
                if (crops?.HasCrop(settings.OpenGraphCropAlias) is true) return content.GetCropUrl(settings.OpenGraphCropAlias, UrlMode.Absolute);
                return content.Url(mode: UrlMode.Absolute);
            }

            return content.Url(mode: UrlMode.Absolute);
        }
    }
}
