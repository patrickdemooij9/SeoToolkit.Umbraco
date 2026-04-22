using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class ExternalSiteAuditCreateRequestDto
    {
        public string Name { get; set; }
        public Uri StartingUrl { get; set; }
        public int? MaxPagesToCrawl { get; set; }
        public int DelayBetweenRequests { get; set; }
        public IReadOnlyCollection<int> CheckIds { get; set; }
    }
}
