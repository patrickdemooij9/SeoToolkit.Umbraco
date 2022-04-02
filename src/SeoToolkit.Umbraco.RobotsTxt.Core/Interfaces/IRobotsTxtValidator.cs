using System.Collections.Generic;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtValidator
    {
        IEnumerable<RobotsTxtValidation> Validate(string content);
    }
}
