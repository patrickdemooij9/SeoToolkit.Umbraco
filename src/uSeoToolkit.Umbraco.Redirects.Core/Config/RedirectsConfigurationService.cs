using Microsoft.Extensions.Options;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.Redirects.Core.Config.Models;

namespace uSeoToolkit.Umbraco.Redirects.Core.Config
{
    public class RedirectsConfigurationService : DefaultAppSettingsService<RedirectsConfigModel>
    {
        private readonly IOptionsMonitor<RedirectsAppSettingsModel> _config;

        public RedirectsConfigurationService(IOptionsMonitor<RedirectsAppSettingsModel> config)
        {
            _config = config;
        }

        public override RedirectsConfigModel GetSettings()
        {
            var settings = _config.CurrentValue;
            return new RedirectsConfigModel
            {
                DisabledModules = settings.DisabledModules
            };
        }
    }
}
