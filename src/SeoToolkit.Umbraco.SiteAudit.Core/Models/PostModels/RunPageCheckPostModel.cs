#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class RunPageCheckPostModel
    {
        public Guid ContentId { get; set; }

        /// <summary>
        /// Which language of the page to check. Left null for a page that does not vary, which
        /// lets Umbraco pick the url as it always has.
        /// </summary>
        public string? Culture { get; set; }
    }
}
