using Newtonsoft.Json;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Models.ViewModels
{
    public class RobotsTxtValidationViewModel
    {
        [JsonProperty("lineNumber")]
        public int LineNumber { get; }

        [JsonProperty("error")]
        public string Error { get; }

        public RobotsTxtValidationViewModel(RobotsTxtValidation model)
        {
            LineNumber = model.LineNumber;
            Error = model.Error;
        }
    }
}
