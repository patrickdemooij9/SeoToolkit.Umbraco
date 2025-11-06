using System;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business
{
    public class RobotsTxtModel
    {
        [Obsolete("Use Key property instead")]
        public int Id { get; set; }
        public Guid Key { get; set; }
        public string Content { get; set; }
        public int? DomainId { get; set; }
    }
}
