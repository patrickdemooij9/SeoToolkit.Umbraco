using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Interfaces
{
    public interface ILinkParser
    {
        IEnumerable<Uri> GetLinks(CrawledPageModel page);
    }
}
