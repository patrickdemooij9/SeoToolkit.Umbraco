using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.MetaFields.AI.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.MetaFields.AI.Composers
{
    public class MetaFieldsAIComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IMetaFieldsAIService, MetaFieldsAIService>();
        }
    }
}
