using Umbraco.Extensions;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Repositories
{
    public class SiteCheckDatabaseRepository : ISiteCheckRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public SiteCheckDatabaseRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public int Get(string alias)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var entity = scope.Database.FirstOrDefault<SiteCheckEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SiteCheckEntity>()
                .Where<SiteCheckEntity>(it => it.Alias == alias));
            return entity?.Id ?? 0;
        }

        public int RegisterCheck(ISiteCheck check)
        {
            var entity = new SiteCheckEntity {Alias = check.Alias};
            using var scope = _scopeProvider.CreateScope();
            scope.Database.Insert(entity);
            scope.Complete();
            return entity.Id;
        }
    }
}
