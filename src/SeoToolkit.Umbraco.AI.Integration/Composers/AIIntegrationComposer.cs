using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.AI.Core.Services;
using SeoToolkit.Umbraco.AI.Integration.Services;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.AI.Integration.Composers
{
    public class AIIntegrationComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IAIGenerationService, UmbracoAIGenerationService>();

            AIHelper.IsAIEnabled = true;
        }
    }
}
