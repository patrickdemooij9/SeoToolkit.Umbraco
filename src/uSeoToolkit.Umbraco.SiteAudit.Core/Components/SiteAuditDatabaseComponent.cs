using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using uSeoToolkit.Umbraco.SiteAudit.Core.Migrations;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Components
{
    public class SiteAuditDatabaseComponent : IComponent
    {
        private readonly IMigrationPlanExecutor _planExecutor;
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueService _keyValueService;

        public SiteAuditDatabaseComponent(IMigrationPlanExecutor planExecutor, IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger<SiteAuditDatabaseComponent> logger)
        {
            _planExecutor = planExecutor;
            _scopeProvider = scopeProvider;
            _keyValueService = keyValueService;
        }

        public void Initialize()
        {
            var plan = new MigrationPlan("uSeoToolkit_SiteAudit_Migration");
            plan.From(string.Empty)
                .To<SiteAuditInitialMigration>("state-1");

            var upgrader = new Upgrader(plan);
            upgrader.Execute(_planExecutor, _scopeProvider, _keyValueService);
        }

        public void Terminate()
        {
        }
    }
}
