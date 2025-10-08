using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Startup;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Api
{
    public class ScriptManagerApiHandler : IApiDataHandler
    {
        private readonly IScriptManagerService _scriptManagerService;
        private readonly ISeoDomainResolver _seoDomainResolver;
        private readonly ISettingsService<ScriptManagerConfigModel> _settings;

        public string Name => "scripts";

        public ScriptManagerApiHandler(IScriptManagerService scriptManagerService, ISeoDomainResolver seoDomainResolver, ISettingsService<ScriptManagerConfigModel> settings)
        {
            _scriptManagerService = scriptManagerService;
            _seoDomainResolver = seoDomainResolver;
            _settings = settings;
        }

        public object GetData(IPublishedContent content)
        {
            var domain = _seoDomainResolver.ResolveDomain();
            if (domain != null && !domain.HasFunctionality($"Module.{ScriptManagerTreeSection.SectionGuid}"))
            {
                domain = null;
            }

            if (_settings.GetSettings().RenderScriptsInApi)
            {
                var model = _scriptManagerService.GetRender(domain?.Id);
                return new ScriptRenderApiModel
                {
                    HeadBottom = model.Get(Enums.ScriptPositionType.HeadBottom).ToArray(),
                    BodyTop = model.Get(Enums.ScriptPositionType.BodyTop).ToArray(),
                    BodyBottom = model.Get(Enums.ScriptPositionType.BodyBottom).ToArray()
                };
            }

            var scripts = _scriptManagerService.GetAll(domain?.Id).ToArray();
            return scripts.Select(it => new ScriptApiModel
            {
                DefinitionAlias = it.Definition.Alias,
                Config = it.Config
            });
        }
    }
}
