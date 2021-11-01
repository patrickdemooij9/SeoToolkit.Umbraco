using System;
using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface IScheduler
    {
        int Count { get; }
        int TotalCount { get; }
        void Add(Uri pageToCrawl);
        void Add(IEnumerable<Uri> pagesToCrawl);
        Uri GetNext();
        void AddKnownUri(Uri uri);
        bool IsUriKnown(Uri uri);
    }
}
