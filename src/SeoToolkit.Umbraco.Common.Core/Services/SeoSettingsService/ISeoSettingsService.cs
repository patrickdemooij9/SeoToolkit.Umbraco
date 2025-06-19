using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public interface ISeoSettingsService
    {
        bool IsEnabled(IContentType contentType);
        bool SupressContentAppSavingNotification();
        void ToggleSeoSettings(int contentTypeId, bool value);
    }
}
