using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Caching;
using SeoToolkit.Umbraco.ScriptManager.Core.Constants;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Services
{
    public class ScriptManagerService : IScriptManagerService
    {
        private readonly IScriptRepository _scriptRepository;
        private readonly DistributedCache _distributedCache;
        private readonly IAppPolicyCache _cache;

        public ScriptManagerService(IScriptRepository scriptRepository, AppCaches appCaches, DistributedCache distributedCache)
        {
            _scriptRepository = scriptRepository;
            _distributedCache = distributedCache;
            _cache = appCaches.RuntimeCache;
        }

        public Script Save(Script script)
        {
            if (script.Id == 0)
            {
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

        public IEnumerable<Script> GetAll()
        {
            return _cache.GetCacheItem($"{CacheConstants.ScriptManager}GetAll", () =>
            {
                return _scriptRepository.GetAll().Where(it => it.Definition != null).ToArray();
            });
        }

        public Script Get(int id)
        {
            return _cache.GetCacheItem($"{CacheConstants.ScriptManager}Get_{id}", () => _scriptRepository.Get(id));
        }

        public ScriptRenderModel GetRender()
        {
            return _cache.GetCacheItem($"{CacheConstants.ScriptManager}GetRender", () =>
            {
                var renderModel = new ScriptRenderModel();
                foreach (var script in GetAll())
                {
                    script.Definition.Render(renderModel, script.Config);
                }

                return renderModel;
            });
        }

        private void ClearCache()
        {
            _distributedCache.RefreshAll(ScriptManagerCacheRefresher.CacheGuid);
        }
    }
}
