using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.Core.Sections;

namespace uSeoToolkit.Umbraco.Core.Composers
{
    public class USeoToolkitComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().Append<USeoToolkitSection>();
        }
    }
}
