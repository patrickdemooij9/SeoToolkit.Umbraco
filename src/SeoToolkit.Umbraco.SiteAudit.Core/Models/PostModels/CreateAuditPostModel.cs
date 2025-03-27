using System;
using System.ComponentModel.DataAnnotations;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class CreateAuditPostModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid SelectedNodeId { get; set; }
        public int[] Checks { get; set; }
        public bool StartAudit { get; set; }
        public int MaxPagesToCrawl { get; set; }
        public int DelayBetweenRequests { get; set; }
    }
}
