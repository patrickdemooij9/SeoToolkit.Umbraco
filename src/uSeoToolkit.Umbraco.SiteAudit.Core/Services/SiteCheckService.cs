using System;
using System.Collections.Generic;
using System.Linq;
using uSeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using uSeoToolkit.Umbraco.SiteAudit.Core.Collections;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Config;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public class SiteCheckService : ISiteCheckService
    {
        private readonly ISiteCheckRepository _siteCheckRepository;
        private readonly SiteAuditCheckCollection _checkCollection;
        private readonly SiteAuditConfigModel _settings;

        private List<SiteCheckDto> _items { get; set; }

        public SiteCheckService(ISiteCheckRepository siteCheckRepository,
            ISettingsService<SiteAuditConfigModel> settingsService,
            SiteAuditCheckCollection checkCollection)
        {
            _siteCheckRepository = siteCheckRepository;
            _checkCollection = checkCollection;
            _settings = settingsService.GetSettings();
        }

        public IEnumerable<SiteCheckDto> GetAll()
        {
            if (_items != null)
                return _items;

            LoadItems();

            return _items;
        }

        public SiteCheckDto GetCheckById(int id)
        {
            LoadItems();
            return _items.FirstOrDefault(it => it.Id == id);
        }

        private void LoadItems()
        {
            if (_items != null)
                return;

            _items = new List<SiteCheckDto>();
            foreach (var codeCheck in _checkCollection)
            {
                //If we cannot find the settings for it, just use default
                var settingsCheck = _settings.Checks.FirstOrDefault(it => it.Alias.Equals(codeCheck.Alias, StringComparison.InvariantCultureIgnoreCase));
                var registeredCheckId = _siteCheckRepository.Get(codeCheck.Alias);
                if (registeredCheckId == 0)
                {
                    registeredCheckId = _siteCheckRepository.RegisterCheck(codeCheck);
                }

                if (settingsCheck is null || settingsCheck.Enabled)
                    _items.Add(new SiteCheckDto { Id = registeredCheckId, Check = codeCheck });
            }
        }
    }
}
