using System.Collections.Generic;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services
{
    public interface IScriptManagerService
    {
        Script Save(Script script);
        void Delete(int[] ids);
        IEnumerable<Script> GetAll();
        Script Get(int id);
        ScriptRenderModel GetRender();
    }
}
