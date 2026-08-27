using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    /// <summary>
    /// Shared url/status state handed to legacy <see cref="Interfaces.ISiteCheck"/> checks.
    /// The two lookup methods are virtual so the new crawler can back them with its own index
    /// instead of this dictionary, which is not safe to write to from several threads.
    /// </summary>
    public class SiteAuditContext
    {
        public Dictionary<Uri, int> RequestedUrls { get; }

        public SiteAuditContext()
        {
            RequestedUrls = new Dictionary<Uri, int>();
        }

        public virtual void AddUrlIfNotPresent(Uri url, int statusCode)
        {
            if (RequestedUrls.ContainsKey(url)) return;
            RequestedUrls.Add(url, statusCode);
        }

        public virtual int? GetStatusCode(Uri url)
        {
            return RequestedUrls.ContainsKey(url) ? RequestedUrls[url] : null;
        }
    }
}
