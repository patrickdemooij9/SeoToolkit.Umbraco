namespace SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository
{
    public interface ISeoSettingsRepository
    {
        bool IsEnabled(int contentTypeId);
        void Toggle(int contentTypeId, bool value);
    }
}
