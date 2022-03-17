using System;

namespace uSeoToolkit.Umbraco.Redirects.Core.Models.PostModels
{
    public class SaveRedirectPostModel
    {
        public int Id { get; set; }
        
        public int? Domain { get; set; }
        
        public string CustomDomain { get; set; }
        
        public bool IsRegex { get; set; }
        
        public string OldUrl { get; set; }
        
        public string NewUrl { get; set; }
        
        public int? NewNodeId { get; set; }
        
        public int? NewCultureId { get; set; }
        
        public int RedirectCode { get; set; }
    }
}
