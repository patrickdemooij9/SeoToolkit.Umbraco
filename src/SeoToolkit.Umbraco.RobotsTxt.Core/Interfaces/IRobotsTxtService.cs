using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtService
    {
        string GetContent();
        string GetContentWithSitemaps(HttpRequest request);
        void SetContent(string content);

        IEnumerable<RobotsTxtValidation> Validate(string content);
    }
}
