using System.Collections.Generic;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace SeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsRepository
    {
        void Save(Redirect redirect);
        void UpdateRedirectCodes(int[] ids, int redirectCode);
        void Delete(Redirect redirect);

        Redirect Get(int id);
        Redirect[] Get(params int[] ids);
        IEnumerable<Redirect> GetAll(int pageNumber, int pageSize, out long totalRecords, string orderBy = null, string orderDirection = null, string search = "");
        IEnumerable<Redirect> GetAllRegexRedirects();
        IEnumerable<Redirect> GetByUrls(params string[] paths);
    }
}
