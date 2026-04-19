using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Caching;
using SeoToolkit.Umbraco.ScriptManager.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.ScriptManager.Core.Config.Models;
using System;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Services
{
    public class ScriptManagerService : IScriptManagerService
    {
        private readonly IScriptRepository _scriptRepository;
        private readonly DistributedCache _distributedCache;
        private readonly ISettingsService<ScriptManagerConfigModel> _settings;
        private readonly AppCaches _cache;

        public ScriptManagerService(IScriptRepository scriptRepository,
            AppCaches appCaches,
            DistributedCache distributedCache,
            ISettingsService<ScriptManagerConfigModel> settings)
        {
            _scriptRepository = scriptRepository;
            _distributedCache = distributedCache;
            _settings = settings;
            _cache = appCaches;
        }

        public Script Save(Script script)
        {
            if (script.Key is null)
            {
                if (script.SortOrder <= 0)
                {
                    script.SortOrder = _scriptRepository.GetMaxSortOrder(script.DomainId) + 1;
                }

                script.Key = Guid.NewGuid();
                script = _scriptRepository.Add(script);
            }
            else
            {
                script = _scriptRepository.Update(script);
            }

            ClearCache();
            return script;
        }

        public void Delete(int[] ids)
        {
            foreach (var id in ids)
            {
                var script = Get(id);
                if (script is null) continue;

                _scriptRepository.Delete(script);
            }

            ClearCache();
        }

        public void Delete(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var script = Get(id);
                if (script is null) continue;

                _scriptRepository.Delete(script);
            }

            ClearCache();
        }

        public IEnumerable<Script> GetAll(Guid? domainId)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.ScriptManager}GetAll_{domainId}", () =>
            {
                return _scriptRepository.GetAll(domainId).Where(it => it.Definition != null).ToArray();
            });
        }

        public Script Get(int id)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.ScriptManager}Get_{id}", () => _scriptRepository.Get(id));
        }

        public Script Get(Guid id)
        {
            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.ScriptManager}Get_{id}", () => _scriptRepository.Get(id));
        }

        public ScriptRenderModel GetRender(Guid? domainId)
        {
            if (_settings.GetSettings().DisableRenderCaching)
                return DoGetRender(domainId);

            return _cache.RuntimeCache.GetCacheItem($"{CacheConstants.ScriptManager}GetRender_{domainId}", () => DoGetRender(domainId));
        }

        private ScriptRenderModel DoGetRender(Guid? domainId)
        {
            var renderModel = new ScriptRenderModel();
            foreach (var script in GetAll(domainId))
            {
                script.Definition.Render(renderModel, script.Config);
            }

            return renderModel;
        }

        private void ClearCache()
        {
            _distributedCache.RefreshAll(ScriptManagerCacheRefresher.CacheGuid);
        }
    }
}
