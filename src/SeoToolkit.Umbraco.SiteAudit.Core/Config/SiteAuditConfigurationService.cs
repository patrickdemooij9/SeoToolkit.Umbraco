using System.Linq;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Config
{
    public class SiteAuditConfigurationService : DefaultAppSettingsService<SiteAuditConfigModel>
    {
        private readonly IOptionsMonitor<SiteAuditAppSettingsModel> _config;

        public SiteAuditConfigurationService(IOptionsMonitor<SiteAuditAppSettingsModel> config)
        {
            _config = config;
        }


        public override SiteAuditConfigModel GetSettings()
        {
            var settings = _config.CurrentValue;
            return new SiteAuditConfigModel
            {
                AllowMinimumDelayBetweenRequestSetting = settings.AllowMinimumDelayBetweenRequestSetting,
                MinimumDelayBetweenRequest = settings.MinimumDelayBetweenRequest,
                Checks = settings.Checks.Select(it => new SiteAuditCheckConfigModel
                {
                    Alias = it.Key,
                    Enabled = it.Value.Enabled
                }).ToArray()
            };
        }
    }
}
