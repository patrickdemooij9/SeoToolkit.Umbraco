using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Startup
{
    public class SiteAuditTreeSection : ISeoTreeSection
    {
        public Guid Id => new("B0D1C655-472B-40E7-9AC4-C6328EA9CF32");

        public string Name => "Site Audit";
        public bool CanBeDomainSpecific => false;
    }
}
