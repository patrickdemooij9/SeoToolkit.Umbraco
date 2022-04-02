using Newtonsoft.Json;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class StopAuditPostModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
