using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class MultiplePublishedContentSeoValueConverter : ISeoValueConverter
    {
        public Type FromValue => typeof(IEnumerable<IPublishedContent>);
        public Type ToValue => typeof(string);

        private ISettingsService<MetaFieldsConfigModel> _settingsService;

        public MultiplePublishedContentSeoValueConverter(ISettingsService<MetaFieldsConfigModel> settingsService)
        {
            _settingsService = settingsService;
        }

        public object Convert(object value, IPublishedContent currentContent, string fieldAlias)
        {
            if (value is not IEnumerable<IPublishedContent> castedValue)
                return null;

            var settings = _settingsService.GetSettings();
            foreach(var item in castedValue)
            {
                if (item.ItemType == PublishedItemType.Media)
                {
                    if (!settings.SupportedMediaTypes.Contains(Path.GetExtension(item.Url()), StringComparer.InvariantCultureIgnoreCase)) continue;

                    if (string.IsNullOrWhiteSpace(settings.OpenGraphCropAlias)) return item.GetCropUrl(urlMode: UrlMode.Absolute);

                    var crops = item.Value<ImageCropperValue>("umbracoFile");
                    if (crops?.HasCrop(settings.OpenGraphCropAlias) is true) return item.GetCropUrl(settings.OpenGraphCropAlias, UrlMode.Absolute);
                    return item.GetCropUrl(urlMode: UrlMode.Absolute);
                }

                return item.Url(mode: UrlMode.Absolute);
            }
            return null;
        }
    }
}
