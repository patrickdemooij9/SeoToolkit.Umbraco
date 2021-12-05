using System.Collections.Generic;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services
{
    public interface IScriptManagerService
    {
        IEnumerable<Script> GetAll();
    }
}
