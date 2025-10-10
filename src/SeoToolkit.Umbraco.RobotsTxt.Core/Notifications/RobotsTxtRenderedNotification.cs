using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Notifications;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Notifications
{
    public class RobotsTxtRenderedNotification : INotification
    {
        public string Content { get; set; }
        public HttpContext HttpContext { get; }

        public RobotsTxtRenderedNotification(string content, HttpContext httpContext)
        {
            Content = content;
            HttpContext = httpContext;
        }
    }
}
