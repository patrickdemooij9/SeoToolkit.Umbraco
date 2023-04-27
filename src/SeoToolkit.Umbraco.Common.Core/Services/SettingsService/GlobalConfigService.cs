using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using System;

namespace SeoToolkit.Umbraco.Common.Core.Services.SettingsService
{
    public class GlobalConfigService : DefaultAppSettingsService<GlobalConfig>
    {
        private readonly IOptionsMonitor<GlobalAppSettingsModel> _config;

        public GlobalConfigService(IOptionsMonitor<GlobalAppSettingsModel> config)
        {
            _config = config;
        }

        public override GlobalConfig GetSettings()
        {
            var settings = _config.CurrentValue;
            return new GlobalConfig
            {
                AutomaticSitemapsInRobotsTxt = settings.AutomaticSitemapsInRobotsTxt,
                EnableSeoSettingsByDefault = settings.EnableSeoSettingsByDefault
            };
        }
    }
}
