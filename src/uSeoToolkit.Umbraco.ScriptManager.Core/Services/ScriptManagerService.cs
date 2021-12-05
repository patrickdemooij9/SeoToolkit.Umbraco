using System.Collections.Generic;
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

        public IEnumerable<Script> GetAll()
        {
            return _scriptRepository.GetAll();
        }
    }
}
