using System.Collections.Generic;
using uSeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IScriptRepository
    {
        void Add(Script script);
        void Update(Script script);
        void Delete(Script script);

        Script Get(int id);
        IEnumerable<Script> GetAll();
    }
}
