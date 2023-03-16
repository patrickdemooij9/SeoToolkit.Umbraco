using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.uSync.Core.Serializers;
using SeoToolkit.Umbraco.uSync.Core.XmlTrackers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSync.Core.Tracking;

namespace SeoToolkit.Umbraco.uSync.Core.Composers;

public class USyncComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {

        builder.Services.AddTransient<MetaFieldValuesSerializer, MetaFieldValuesSerializer>();
        builder.Services.AddTransient<MetaFieldSettingsSerializer, MetaFieldSettingsSerializer>();
        builder.Services.AddTransient<ISyncTrackerBase, SeoToolkitXmlTracker>();
    }
}