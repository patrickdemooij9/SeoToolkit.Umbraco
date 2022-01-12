using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Services;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Middleware
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

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals("/robots.txt", StringComparison.InvariantCultureIgnoreCase))
            {
                await _next.Invoke(context);
                return;
            }

            var robotsTxt = _robotsTxtService.GetContent();
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
