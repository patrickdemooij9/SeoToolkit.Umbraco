#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class CreateAuditPostModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The node to start from. Optional: a crawl can equally be aimed at a url directly,
        /// which is the only workable option for a decoupled frontend whose routing does not
        /// follow the content tree.
        /// </summary>
        public Guid? SelectedNodeId { get; set; }

        /// <summary>
        /// Which language of the node to start from. Only meaningful with
        /// <see cref="SelectedNodeId"/>, and only when that node is published in more than one.
        /// </summary>
        public string? Culture { get; set; }

        /// <summary>
        /// The url to start from, used when no node is given. Crawled exactly as supplied - no
        /// domain BaseUrl is applied, because the point of typing one is to reach somewhere the
        /// configuration does not describe.
        /// </summary>
        public string? StartingUrl { get; set; }

        /// <summary>
        /// Aliases of the checks to run. Aliases rather than ids, so a check from an add-on
        /// package can be selected without ever having been written to the database.
        /// </summary>
        public string[]? Checks { get; set; }

        public bool StartAudit { get; set; }
        public int MaxPagesToCrawl { get; set; }
        public int DelayBetweenRequests { get; set; }
    }
}
