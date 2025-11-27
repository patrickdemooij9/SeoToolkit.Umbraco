using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Services
{
    public interface IPageNotFoundService
    {
        void SetPageNotFound(Guid? nodeId, Guid? domainId);
        Guid? GetPageNotFound(Guid? domainId);
    }
}
