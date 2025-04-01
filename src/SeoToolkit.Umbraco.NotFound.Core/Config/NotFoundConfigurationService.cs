using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;

namespace SeoToolkit.Umbraco.NotFound.Core.Config
{
    public class NotFoundConfigurationService : DefaultAppSettingsService<NotFoundConfigModel>
    {
        private readonly IOptionsMonitor<NotFoundAppSettingsModel> _config;

        public NotFoundConfigurationService(IOptionsMonitor<NotFoundAppSettingsModel> config)
        {
            _config = config;
        }

        public override NotFoundConfigModel GetSettings()
        {
            var settings = _config.CurrentValue;
            return new NotFoundConfigModel
            {
                DisabledModules = settings.DisabledModules
            };
        }
    }
}