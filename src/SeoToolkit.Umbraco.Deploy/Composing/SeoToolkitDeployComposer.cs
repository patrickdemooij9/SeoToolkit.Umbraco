using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Deploy.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddOptions<SeoToolkitDeploySettings>()
                .Bind(builder.Config.GetSection("SeoToolkit:Deploy"));

            builder.Components().Append<SeoToolkitDeployComponent>();
        }
    }
}
