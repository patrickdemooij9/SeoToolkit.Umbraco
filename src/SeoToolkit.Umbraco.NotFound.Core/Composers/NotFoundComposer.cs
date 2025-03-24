using SeoToolkit.Umbraco.NotFound.Core.Components;
using SeoToolkit.Umbraco.NotFound.Core.ContentFinders;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.NotFound.Core.Composers;

public class NotFoundComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Components().Append<EnableModuleComponent>();
        builder.SetContentLastChanceFinder<PageNotFoundFinder>();
    }
}
