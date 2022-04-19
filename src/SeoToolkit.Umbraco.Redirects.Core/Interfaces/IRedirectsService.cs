using System;
using Umbraco.Cms.Core.Models;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsService
    {
        PagedResult<Redirect> GetAll(int pageNumber, int pageSize, string orderBy = null, string orderDirection = null, string search = "");
        Redirect Get(int id);
        void Save(Redirect redirect);
        void Delete(int[] ids);

        RedirectFindResult GetByUrl(Uri url);
    }
}
