using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.AI.Core.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.AI.Core.Composers
{
    public class AIComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IMetaFieldsAIService, MetaFieldsAIService>();
        }
    }
}
