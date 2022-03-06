using System;
using System.Collections.Generic;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;

namespace uSeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectsService
    {
        IEnumerable<Redirect> GetAll();
        void Save(Redirect redirect);

        Redirect GetByUrl(Uri url);
    }
}
