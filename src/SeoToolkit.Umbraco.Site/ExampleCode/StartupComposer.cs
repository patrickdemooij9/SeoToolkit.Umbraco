using SeoToolkit.Umbraco.Sitemap.Core.Collections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Site.ExampleCode
{
    public class StartupComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.SitemapCollections().Append<ExampleSitemapCollection>();
        }
    }
}
