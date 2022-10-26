using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;

namespace SeoToolkit.Umbraco.MetaFields.Core.Config
{
    public class MetaFieldsConfigurationService : DefaultAppSettingsService<MetaFieldsConfigModel>
    {
        private readonly IOptionsMonitor<MetaFieldsAppSettingsModel> _config;

        public MetaFieldsConfigurationService(IOptionsMonitor<MetaFieldsAppSettingsModel> config)
        {
            _config = config;
        }

        public override MetaFieldsConfigModel GetSettings()
        {
            var settings = _config.CurrentValue;
            return new MetaFieldsConfigModel
            {
                SupportedMediaTypes = settings.SupportedMediaTypes,
                OpenGraphCropAlias = settings.OpenGraphCropAlias,
                DisabledModules = settings.DisabledModules
            };
        }
    }
}
