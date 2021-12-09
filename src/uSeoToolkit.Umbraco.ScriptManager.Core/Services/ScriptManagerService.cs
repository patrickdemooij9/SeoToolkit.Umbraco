using System.Collections.Generic;
using System.Linq;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces;
using uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Services
{
    public class ScriptManagerService : IScriptManagerService
    {
        private readonly IScriptRepository _scriptRepository;

        public ScriptManagerService(IScriptRepository scriptRepository)
        {
            _scriptRepository = scriptRepository;
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
        }

        public IEnumerable<Script> GetAll()
        {
            return _scriptRepository.GetAll().Where(it => it.Definition != null);
        }

        public Script Get(int id)
        {
            return _scriptRepository.Get(id);
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
    }
}
