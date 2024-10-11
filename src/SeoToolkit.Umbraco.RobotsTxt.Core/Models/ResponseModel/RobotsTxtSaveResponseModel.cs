using SeoToolkit.Umbraco.RobotsTxt.Core.Models.ViewModels;
using System.Text.Json.Serialization;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Models.ResponseModel
{
    public class RobotsTxtSaveResponseModel
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("errors")]
        public RobotsTxtValidationViewModel[] Errors { get; set; }
    }
}
