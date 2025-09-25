using SeoToolkit.Umbraco.Common.Core.Models.Business;
using System;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public interface ISeoDomainResolver
    {
        SeoDomainCollection? ResolveSeoDomain(Uri requestUrl);
    }
}
