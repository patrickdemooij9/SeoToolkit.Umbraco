using Newtonsoft.Json;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.PostModels
{
    public class DeleteAuditsPostModel
    {
        [JsonProperty("ids")]
        public int[] Ids { get; set; }
    }
}
