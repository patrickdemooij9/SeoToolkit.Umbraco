using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Services
{
    public class ScriptManagerService : IScriptManagerService
    {
        private const string BaseCacheKey = "ScriptManager_";

        private readonly IScriptRepository _scriptRepository;
        private readonly IAppPolicyCache _cache;

        public ScriptManagerService(IScriptRepository scriptRepository, AppCaches appCaches)
        {
            _scriptRepository = scriptRepository;
            _cache = appCaches.RuntimeCache;
        }

        public void Save(Script script)
        {
            if (script.Id == 0)
            {
                _scriptRepository.Add(script);
            }
            else
            {
                _scriptRepository.Update(script);
            }

            ClearCache();
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
            return _cache.GetCacheItem($"{BaseCacheKey}GetAll", () =>
            {
                return _scriptRepository.GetAll().Where(it => it.Definition != null);
            });
        }

        public Script Get(int id)
        {
            return _cache.GetCacheItem($"{BaseCacheKey}Get_{id}", () => _scriptRepository.Get(id));
        }

        public ScriptRenderModel GetRender()
        {
            var renderModel = new ScriptRenderModel();
            foreach (var script in GetAll())
            {
                script.Definition.Render(renderModel, script.Config);
            }

            return renderModel;
        }

        private void ClearCache()
        {
            _cache.ClearByKey(BaseCacheKey);
        }
    }
}
