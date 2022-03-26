using System.Collections.Generic;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsRepository
    {
        void Save(Redirect redirect);
        void Delete(Redirect redirect);

        Redirect Get(int id);
        IEnumerable<Redirect> GetAll(int pageNumber, int pageSize, out long totalRecords);
        IEnumerable<Redirect> GetAllRegexRedirects();
        IEnumerable<Redirect> GetByUrls(params string[] paths);
    }
}
