using System;
using System.Collections.Generic;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsService
    {
        IEnumerable<Redirect> GetAll();
        Redirect Get(int id);
        void Save(Redirect redirect);
        void Delete(int[] ids);

        RedirectFindResult GetByUrl(Uri url);
    }
}
