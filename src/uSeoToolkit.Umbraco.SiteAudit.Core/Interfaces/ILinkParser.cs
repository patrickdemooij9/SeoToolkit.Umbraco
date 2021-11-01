using System;
using System.Collections.Generic;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ILinkParser
    {
        IEnumerable<Uri> GetLinks(CrawledPageModel page);
    }
}
