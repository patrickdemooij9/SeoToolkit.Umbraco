using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Api
{
    public class ScriptManagerApiHandler : IApiDataHandler
    {
        private readonly IScriptManagerService _scriptManagerService;
        private readonly ISettingsService<ScriptManagerConfigModel> _settings;

        public string Name => "scripts";

        public ScriptManagerApiHandler(IScriptManagerService scriptManagerService, ISettingsService<ScriptManagerConfigModel> settings)
        {
            _scriptManagerService = scriptManagerService;
            _settings = settings;
        }

        public object GetData(IPublishedContent content)
        {
            if (_settings.GetSettings().RenderScriptsInApi)
            {
                var model = _scriptManagerService.GetRender();
                return new ScriptRenderApiModel
                {
                    HeadBottom = model.Get(Enums.ScriptPositionType.HeadBottom).ToArray(),
                    BodyTop = model.Get(Enums.ScriptPositionType.BodyTop).ToArray(),
                    BodyBottom = model.Get(Enums.ScriptPositionType.BodyBottom).ToArray()
                };
            }

            var scripts = _scriptManagerService.GetAll().ToArray();
            return scripts.Select(it => new ScriptApiModel
            {
                DefinitionAlias = it.Definition.Alias,
                Config = it.Config
            });
        }
    }
}
