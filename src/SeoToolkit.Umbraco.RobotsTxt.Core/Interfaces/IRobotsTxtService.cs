using System.Collections.Generic;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtService
    {
        string GetContent();
        void SetContent(string content);

        IEnumerable<RobotsTxtValidation> Validate(string content);
    }
}
