using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.Common.Core.Collections;
using uSeoToolkit.Umbraco.Common.Core.Dashboards;
using uSeoToolkit.Umbraco.Common.Core.Sections;

namespace uSeoToolkit.Umbraco.Common.Core.Composers
{
    public class USeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<USeoToolkitSection>();
            builder.Dashboards().Add<WelcomeDashboard>();

            builder.Services.AddSingleton<ModuleCollection>();
        }
    }
}
