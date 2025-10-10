using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Services;
using SeoToolkit.Umbraco.RobotsTxt.Core.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Middleware
{
    public class RobotsTxtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRobotsTxtService _robotsTxtService;

        public RobotsTxtMiddleware(RequestDelegate next, IRobotsTxtService robotsTxtService)
        {
            _next = next;
            _robotsTxtService = robotsTxtService;
        }

        public async Task Invoke(HttpContext context, IEventAggregator notificationPublisher)
        {
            if (!context.Request.Path.Equals("/robots.txt", StringComparison.InvariantCultureIgnoreCase))
            {
                await _next.Invoke(context);
                return;
            }

            var robotsTxt = _robotsTxtService.GetContentWithSitemaps(context.Request);
            
            // Fire notification so content can be changed before returning
            await notificationPublisher.PublishAsync(new RobotsTxtRenderedNotification(robotsTxt, context));
            robotsTxt = notification.Content;

            if (string.IsNullOrWhiteSpace(robotsTxt))
            {
                await _next.Invoke(context);
                return;
            }

            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(robotsTxt);
        }
    }
}
