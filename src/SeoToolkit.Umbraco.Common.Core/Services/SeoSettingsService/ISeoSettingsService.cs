using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public interface ISeoSettingsService
    {
        bool IsEnabled(IPublishedContentType contentType);
        bool SupressContentAppSavingNotification();
        void ToggleSeoSettings(int contentTypeId, bool value);
    }
}
