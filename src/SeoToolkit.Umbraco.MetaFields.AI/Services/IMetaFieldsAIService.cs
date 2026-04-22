using SeoToolkit.Umbraco.MetaFields.AI.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.MetaFields.AI.Services
{
    public interface IMetaFieldsAIService
    {
        Task<MetaFieldsAIGenerateResponseModel> GenerateAsync(
            IPublishedContent content,
            string culture,
            CancellationToken cancellationToken = default);
    }
}
