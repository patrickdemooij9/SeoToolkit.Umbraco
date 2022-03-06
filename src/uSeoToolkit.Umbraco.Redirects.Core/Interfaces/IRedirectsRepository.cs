using System.Collections.Generic;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsRepository
    {
        void Save(Redirect redirect);
        IEnumerable<Redirect> GetAll();
        IEnumerable<Redirect> GetByUrls(int domainId, params string[] paths);
        IEnumerable<Redirect> GetByUrls(string customDomain, params string[] paths);
    }
}
