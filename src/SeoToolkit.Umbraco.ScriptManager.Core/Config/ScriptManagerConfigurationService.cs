using System.Linq;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Config
{
    public class ScriptManagerConfigurationService : DefaultAppSettingsService<ScriptManagerConfigModel>
    {
        private readonly IOptionsMonitor<ScriptManagerAppSettingsModel> _config;

        public ScriptManagerConfigurationService(IOptionsMonitor<ScriptManagerAppSettingsModel> config)
        {
            _config = config;
        }

        public override ScriptManagerConfigModel GetSettings()
        {
            var settings = _config.CurrentValue;
            return new ScriptManagerConfigModel
            {
                Definitions = settings.Definitions.Select(it => new ScriptManagerDefinitionConfigModel
                {
                    Alias = it.Key,
                    Enabled = it.Value.Enabled
                }).ToArray(),
                DisabledModules = settings.DisabledModules
            };
        }
    }
}
