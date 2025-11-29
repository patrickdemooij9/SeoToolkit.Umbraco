using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi;

namespace SeoToolkit.Umbraco.Common.Core.Swagger
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc(
            "seoToolkit",
            new OpenApiInfo
            {
                Title = "SeoToolkit",
                Version = "Latest",
            });
        }
    }
}
