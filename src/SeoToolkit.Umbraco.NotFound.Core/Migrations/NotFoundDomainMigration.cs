using SeoToolkit.Umbraco.Common.Core.Models.Database;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SeoToolkit.Umbraco.NotFound.Core.Migrations
{
    public class NotFoundDomainMigration : AsyncMigrationBase
    {
        private readonly IKeyValueRepository _keyValueRepository;
        private readonly ISeoKeyValueRepository _seoKeyValueRepository;

        public NotFoundDomainMigration(IMigrationContext context, IKeyValueRepository keyValueRepository, ISeoKeyValueRepository seoKeyValueRepository) : base(context)
        {
            _keyValueRepository = keyValueRepository;
            _seoKeyValueRepository = seoKeyValueRepository;
        }

        protected override Task MigrateAsync()
        {
            // Dependency on this migration from SeoToolkit.Common.Core
            if (!TableExists("SeoToolkitSeoKeyValues"))
            {
                Create.Table<SeoKeyValueEntity>().Do();
            }

            var keyValue = _keyValueRepository.Get(NotFoundConstants.NotFoundKeyValueKey);
            if (keyValue is null || string.IsNullOrWhiteSpace(keyValue.Value))
            {
                return Task.CompletedTask;
            }

            _seoKeyValueRepository.Set(NotFoundConstants.NotFoundKeyValueKey, keyValue.Value, null);
            return Task.CompletedTask;
        }
    }
}
