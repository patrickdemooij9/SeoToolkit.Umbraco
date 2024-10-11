using System.Text.Json.Serialization;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Models.PostModel
{
    public class RobotsTxtSavePostModel
    {
        [JsonPropertyName("skipValidation")]
        public bool SkipValidation { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
