using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using uSeoToolkit.Umbraco.ScriptManager.Core.Migrations;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Components
{
    public class ScriptManagerDatabaseComponent : IComponent
    {
        private readonly IMigrationPlanExecutor _planExecutor;
        private readonly IScopeProvider _scopeProvider;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public ScriptManagerDatabaseComponent(IMigrationPlanExecutor planExecutor, IScopeProvider scopeProvider, IKeyValueService keyValueService, IRuntimeState runtimeState)
        {
            _planExecutor = planExecutor;
            _scopeProvider = scopeProvider;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level != RuntimeLevel.Run) return;

            var plan = new MigrationPlan("uSeoToolkit_ScriptManager_Migration");
            plan.From(string.Empty)
                .To<ScriptManagerInitialMigration>("state-1");

            var upgrader = new Upgrader(plan);
            upgrader.Execute(_planExecutor, _scopeProvider, _keyValueService);
        }

        public void Terminate()
        {
        }
    }
}
