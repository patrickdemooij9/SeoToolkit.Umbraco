using SeoToolkit.Umbraco.AI.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.AI.Core.Services
{
    public interface IMetaFieldsAIService
    {
        Task<MetaFieldsAIGenerateResponseModel> GenerateAsync(
            IPublishedContent content,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
