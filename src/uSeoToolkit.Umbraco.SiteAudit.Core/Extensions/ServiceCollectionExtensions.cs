using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using uSeoToolkit.Umbraco.SiteAudit.Core.Hubs;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHubSignalR(this IServiceCollection services)
        {

            services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                    "SiteAudit",
                    applicationBuilder => { },
                    applicationBuilder => { },
                    applicationBuilder =>
                    {
                        applicationBuilder.UseEndpoints(e =>
                        {
                            var hubRoutes = applicationBuilder.ApplicationServices.GetRequiredService<SiteAuditHubRoutes>();
                            hubRoutes.CreateRoutes(e);
                        });
                    }
                ));
            });

            return services;
        }
    }
}
