using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public interface ISeoSettingsRepository
    {
        bool IsEnabled(IContentType contentType);
        void Toggle(int contentTypeId, bool value);
    }
}
