using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Core.Config;
using SeoToolkit.Umbraco.Core.Connectors;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Core.Startup
{
    internal class SeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:Global");
            builder.Services.Configure<GlobalAppSettingsModel>(section);

            var settings = section?.Get<GlobalAppSettingsModel>() ?? new GlobalAppSettingsModel();
            if (settings.AutomaticSitemapsInRobotsTxt is true)
            {
                builder.Services.AddSingleton<IRobotsTxtSitemapProvider, RobotsSitemapProvider>();
            }
        }
    }
}
