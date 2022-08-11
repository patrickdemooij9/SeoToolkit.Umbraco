using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.uSync.Core.Serializers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.uSync.Core.Composers;

public class USyncComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<MetaFieldValuesSerializer, MetaFieldValuesSerializer>();
    }
}