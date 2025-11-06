using System;
using Umbraco.Cms.Core.Models;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsService
    {
        PagedResult<Redirect> GetAll(int pageNumber, int pageSize, string orderBy = null, string orderDirection = null, string search = "");
        [Obsolete("Use Get method with Guid instead. Will be removed in next major version.")]
        Redirect Get(int id);
        [Obsolete("Use Get method with Guid[] instead. Will be removed in next major version.")]
        Redirect[] Get(params int[] ids);
        Redirect Get(Guid key);
        Redirect[] Get(params Guid[] keys);
        void Save(Redirect redirect);
        void UpdateRedirectCodes(int[] ids, int redirectCode);
        [Obsolete("Use UpdateRedirectCodes(Guid[] keys, int redirectCode) method instead. Will be removed in next major version.")]
        void UpdateRedirectCodes(Guid[] keys, int redirectCode);
        [Obsolete("Use Delete(Guid[] keys) method instead. Will be removed in next major version.")]
        void Delete(int[] ids);
        void Delete(Guid[] keys);

        RedirectFindResult GetByUrl(Uri url);
    }
}
