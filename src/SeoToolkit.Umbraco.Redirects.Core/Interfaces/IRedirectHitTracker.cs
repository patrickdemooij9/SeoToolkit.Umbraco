using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Interfaces
{
    public interface IRedirectHitTracker
    {
        void RegisterHit(Guid redirectKey);
        void Flush();
    }
}
