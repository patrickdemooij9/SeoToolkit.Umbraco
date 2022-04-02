using SeoToolkit.Umbraco.Common.Core.Repositories.SeoSettingsRepository;

namespace SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService
{
    public class SeoSettingsService : ISeoSettingsService
    {
        private readonly ISeoSettingsRepository _seoSettingsRepository;

        public SeoSettingsService(ISeoSettingsRepository seoSettingsRepository)
        {
            _seoSettingsRepository = seoSettingsRepository;
        }

        public bool IsEnabled(int contentTypeId)
        {
            return _seoSettingsRepository.IsEnabled(contentTypeId);
        }

        public void ToggleSeoSettings(int contentTypeId, bool value)
        {
            _seoSettingsRepository.Toggle(contentTypeId, value);
        }
    }
}
