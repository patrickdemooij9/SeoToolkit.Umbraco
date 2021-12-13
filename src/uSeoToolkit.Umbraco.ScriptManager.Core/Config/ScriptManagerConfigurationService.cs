using System.Linq;
using Microsoft.Extensions.Options;
using uSeoToolkit.Umbraco.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.ScriptManager.Core.Config.Models;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Config
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
                }).ToArray()
            };
        }
    }
}
