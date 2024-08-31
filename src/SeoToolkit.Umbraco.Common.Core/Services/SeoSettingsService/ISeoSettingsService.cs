namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public interface ISeoSettingsService
    {
        bool IsEnabled(int contentTypeId);
        bool SupressContentAppSavingNotification();
        void ToggleSeoSettings(int contentTypeId, bool value);
    }
}
