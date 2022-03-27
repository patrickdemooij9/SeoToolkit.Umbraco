using Microsoft.Extensions.Options;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.MetaFields.Core.Config.Models;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Config
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
                DisabledModules = settings.DisabledModules
            };
        }
    }
}
