using Newtonsoft.Json;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class StopAuditPostModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
