using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Middleware
{
    public class RobotsTxtMiddleware
    {
        private readonly RequestDelegate _next;

        public RobotsTxtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
        }
    }
}
