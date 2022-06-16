using Umbraco.Extensions;
using SeoToolkit.Umbraco.Common.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public class SeoSettingsRepository : ISeoSettingsRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SeoSettingsRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public bool IsEnabled(int contentTypeId)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var entity = scope.Database.FirstOrDefault<SeoSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SeoSettingsEntity>()
                    .Where<SeoSettingsEntity>(it => it.ContentTypeId == contentTypeId));

                //Default is disabled.
                if (entity is null)
                    return false;
                return entity.Enabled;
            }
        }

        public void Toggle(int contentTypeId, bool value)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var entity = scope.Database.FirstOrDefault<SeoSettingsEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SeoSettingsEntity>()
                    .Where<SeoSettingsEntity>(it => it.ContentTypeId == contentTypeId));

                if (entity is null)
                    entity = new SeoSettingsEntity { ContentTypeId = contentTypeId };
                entity.Enabled = value;

                scope.Database.Save(entity);
            }
        }
    }
}
