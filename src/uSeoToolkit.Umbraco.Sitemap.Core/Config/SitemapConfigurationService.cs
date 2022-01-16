using Microsoft.Extensions.Options;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.Sitemap.Core.Config.Models;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Config
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
                PriorityFieldAlias = settings.PriorityFieldAlias
            };
        }
    }
}
