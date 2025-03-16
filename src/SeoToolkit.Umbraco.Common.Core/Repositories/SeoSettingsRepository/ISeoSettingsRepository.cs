using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public interface ISeoSettingsRepository
    {
        bool IsEnabled(IPublishedContentType contentType);
        void Toggle(int contentTypeId, bool value);
    }
}
