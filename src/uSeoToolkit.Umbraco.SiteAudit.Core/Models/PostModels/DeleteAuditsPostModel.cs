using Newtonsoft.Json;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class DeleteAuditsPostModel
    {
        [JsonProperty("ids")]
        public int[] Ids { get; set; }
    }
}
