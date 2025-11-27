using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsRepository
    {
        void Save(Redirect redirect);
        [Obsolete("Use UpdateRedirectCodes(Guid[] keys, int redirectCode) method instead. Will be removed in next major version.")]
        void UpdateRedirectCodes(int[] ids, int redirectCode);
        void UpdateRedirectCodes(Guid[] keys, int redirectCode);
        void Delete(Redirect redirect);

        [Obsolete("Use Get method with Guid instead. Will be removed in next major version.")]
        Redirect Get(int id);
        Redirect Get(Guid key);
        [Obsolete("Use Get method with Guid[] instead. Will be removed in next major version.")]
        Redirect[] Get(params int[] ids);
        Redirect[] Get(params Guid[] keys);
        IEnumerable<Redirect> GetAll(int pageNumber, int pageSize, out long totalRecords, string orderBy = null, string orderDirection = null, string search = "");
        IEnumerable<Redirect> GetAllRegexRedirects();
        IEnumerable<Redirect> GetByUrls(params string[] paths);
    }
}
