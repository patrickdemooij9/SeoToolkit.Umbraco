using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IScriptRepository
    {
        Script Add(Script script);
        Script Update(Script script);
        void Delete(Script script);

        [Obsolete("Use Get(Guid id) method instead")]
        Script Get(int id);
        Script Get(Guid id);
        IEnumerable<Script> GetAll(Guid? domainId);
        int GetMaxSortOrder(Guid? domainId);
    }
}
