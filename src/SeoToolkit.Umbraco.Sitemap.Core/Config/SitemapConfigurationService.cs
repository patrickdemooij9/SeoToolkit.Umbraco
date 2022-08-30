using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;

namespace SeoToolkit.Umbraco.Sitemap.Core.Config
{
    public class SitemapConfigurationService : DefaultAppSettingsService<SitemapConfig>
    {
        private readonly IOptions<SitemapAppSettingsModel> _config;

        public SitemapConfigurationService(IOptions<SitemapAppSettingsModel> config)
        {
            _config = config;
        }

        public override SitemapConfig GetSettings()
        {
            var settings = _config.Value;
            return new SitemapConfig
            {
                ShowAlternatePages = settings.ShowAlternatePages,
                LastModifiedFieldAlias = settings.LastModifiedFieldAlias,
                ChangeFrequencyFieldAlias = settings.ChangeFrequencyFieldAlias,
                PriorityFieldAlias = settings.PriorityFieldAlias,
                ReturnContentType = settings.ReturnContentType,
                DisabledModules = settings.DisabledModules
            };
        }
    }
}
