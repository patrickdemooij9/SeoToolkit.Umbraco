using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class SiteAuditContext
    {
        public Dictionary<Uri, int> RequestedUrls { get; }

        public SiteAuditContext()
        {
            RequestedUrls = new Dictionary<Uri, int>();
        }

        public void AddUrlIfNotPresent(Uri url, int statusCode)
        {
            if (RequestedUrls.ContainsKey(url)) return;
            RequestedUrls.Add(url, statusCode);
        }

        public int? GetStatusCode(Uri url)
        {
            return RequestedUrls.ContainsKey(url) ? RequestedUrls[url] : null;
        }
    }
}
