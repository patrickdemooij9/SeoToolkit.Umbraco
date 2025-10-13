using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Services
{
    public interface IPageNotFoundService
    {
        void SetPageNotFound(Guid? nodeId, int? domainId);
        Guid? GetPageNotFound(int? domainId);
    }
}
