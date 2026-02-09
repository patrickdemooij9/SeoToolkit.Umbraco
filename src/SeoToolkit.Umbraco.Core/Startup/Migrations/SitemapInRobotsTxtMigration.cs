using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Core.SeoSettings;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.Core.Startup.Migrations
{
    public class SitemapInRobotsTxtMigration : AsyncMigrationBase
    {
        private readonly GlobalAppSettingsModel _settings;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;

        public SitemapInRobotsTxtMigration(IMigrationContext context,
            IOptions<GlobalAppSettingsModel> options,
            ISeoKeyValueRepository seoKeyValueRepository) : base(context)
        {
            _settings = options.Value;
            _seoKeyValueRepository = seoKeyValueRepository;
        }

        protected override Task MigrateAsync()
        {
            _seoKeyValueRepository.Set(AutomaticSitemapInRobotsTxtSeoSetting.SettingKey, _settings.AutomaticSitemapsInRobotsTxt ? "true" : "false", null);
            return Task.CompletedTask;
        }
    }
}
