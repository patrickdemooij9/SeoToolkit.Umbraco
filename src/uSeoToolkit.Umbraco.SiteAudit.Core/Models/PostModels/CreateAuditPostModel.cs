using System;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class CreateAuditPostModel
    {
        public string Name { get; set; }
        public int SelectedNodeId { get; set; }
        public int[] Checks { get; set; }
        public bool StartAudit { get; set; }
        public int MaxPagesToCrawl { get; set; }
        public int DelayBetweenRequests { get; set; }
    }
}
