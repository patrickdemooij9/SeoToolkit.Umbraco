using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public class SiteCheckService : ISiteCheckService
    {
        private readonly ISiteCheckRepository _siteCheckRepository;
        private readonly SiteAuditCheckCollection _checkCollection;
        private readonly SiteAuditConfigModel _settings;

        private volatile List<SiteCheckDto> _items;
        private readonly object _loadLock = new();

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

            // Requests can load the checks at the same time (e.g. the dashboard tiles), so only one may build the list,
            // and it's only published once complete.
            lock (_loadLock)
            {
                if (_items != null)
                    return;

                _items = BuildItems();
            }
        }

        private List<SiteCheckDto> BuildItems()
        {
            var items = new List<SiteCheckDto>();
            foreach (var codeCheck in _checkCollection)
            {
                //If we cannot find the settings for it, just use default
                var settingsCheck = _settings.Checks.FirstOrDefault(it => it.Alias.Equals(codeCheck.Alias, StringComparison.InvariantCultureIgnoreCase));
                var registeredCheckId = _siteCheckRepository.Get(codeCheck.Alias);
                if (registeredCheckId == 0)
                {
                    registeredCheckId = _siteCheckRepository.RegisterCheck(codeCheck);
                }

                var isBrokenLinkCheck = codeCheck is BrokenLinkCheck; //TODO: Remove this when we have a better way of handling this. Currently the check is based on the amount of crawled pages but we only want 1 for the content checks
                if (settingsCheck is null || settingsCheck.Enabled)
                    items.Add(new SiteCheckDto { Id = registeredCheckId, Check = codeCheck, AllowedAsPageCheck = settingsCheck?.AllowedAsPageCheck ?? !isBrokenLinkCheck });
            }
            return items;
        }
    }
}
