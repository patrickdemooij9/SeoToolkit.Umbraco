using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services
{
    public interface IScriptManagerService
    {
        Script Save(Script script);
        [Obsolete("Use Delete(Guid[] ids) method instead")]
        void Delete(int[] ids);
        void Delete(Guid[] ids);
        IEnumerable<Script> GetAll(Guid? domainId);
        [Obsolete("Use Get(Guid id) method instead")]
        Script Get(int id);
        Script Get(Guid id);
        ScriptRenderModel GetRender(Guid? domainId);
    }
}
