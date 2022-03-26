using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsService
    {
        PagedResult<Redirect> GetAll(int pageNumber, int pageSize);
        Redirect Get(int id);
        void Save(Redirect redirect);
        void Delete(int[] ids);

        RedirectFindResult GetByUrl(Uri url);
    }
}
