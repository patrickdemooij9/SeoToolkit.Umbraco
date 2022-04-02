using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Collections
{
    public class ScriptDefinitionCollection : BuilderCollectionBase<IScriptDefinition>
    {
        private readonly ISettingsService<ScriptManagerConfigModel> _settingsService;

        public ScriptDefinitionCollection(Func<IEnumerable<IScriptDefinition>> items, ISettingsService<ScriptManagerConfigModel> settingsService) : base(items)
        {
            _settingsService = settingsService;
        }

        public IEnumerable<IScriptDefinition> GetAll()
        {
            var config = _settingsService.GetSettings();
            return this.Where(it =>
            {
                var configValue = config.Definitions.FirstOrDefault(d => d.Alias.Equals(it.Alias));
                return configValue is null || configValue.Enabled;
            });
        }

        public IScriptDefinition Get(string alias)
        {
            return GetAll().FirstOrDefault(it => it.Alias.Equals(alias));
        }
    }
}
