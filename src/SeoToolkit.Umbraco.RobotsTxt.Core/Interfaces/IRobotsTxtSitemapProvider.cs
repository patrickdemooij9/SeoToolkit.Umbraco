using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtSitemapProvider
    {
        IEnumerable<string> GetSitemapUrls(HttpRequest request);
    }
}
