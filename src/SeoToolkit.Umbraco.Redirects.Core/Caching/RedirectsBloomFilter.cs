using BloomFilter;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Redirects.Core.Config.Models;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using System;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using static SeoToolkit.Umbraco.Common.Core.Constants.TreeControllerConstants;

namespace SeoToolkit.Umbraco.Redirects.Core.Caching
{
    internal class RedirectsBloomFilter : IRedirectsBloomFilter
    {
        private readonly IRedirectsRepository _redirectsRepository;
        private readonly ISettingsService<RedirectsConfigModel> _settingsService;

        private IBloomFilter _bloomFilter;
        private int _spaceLeft = 1000;
        private bool _forceRebuild = false;

        public bool ShouldRebuild => _forceRebuild || _spaceLeft <= 0;

        public RedirectsBloomFilter(IRedirectsRepository redirectsRepository, ISettingsService<RedirectsConfigModel> settingsService)
        {
            _redirectsRepository = redirectsRepository;
            _settingsService = settingsService;
            _forceRebuild = true;
        }

        public void Add(string url)
        {
            if (!IsEnabled())
            {
                return;
            }

            if (_bloomFilter is null)
            {
                _forceRebuild = true;
                return;
            }

            _bloomFilter.Add(ToKey(url));
            _spaceLeft--;
        }

        public void Remove(string url)
        {
            if (!IsEnabled())
            {
                return;
            }

            _forceRebuild = true;
        }

        public bool Contains(string url)
        {
            if (!IsEnabled() || _bloomFilter is null)
            {
                return true;
            }

            return _bloomFilter.Contains(ToKey(url));
        }

        public void Rebuild()
        {
            if (!IsEnabled())
            {
                return;
            }

            var redirects = _redirectsRepository.GetAll(1, int.MaxValue, out _).ToArray();

            _spaceLeft = 1000;
            _bloomFilter = FilterBuilder.Build(redirects.Length + 1000, 0.0001);
            _bloomFilter.Add(redirects.Select(r => ToKey(r.OldUrl)).ToArray());
        }

        // BloomFilter.NetCore reports the empty string as present in roughly half of all filters
        // above ~1000 items, and "/" is cleaned to "" before lookup - so without the prefix every
        // homepage request could fall through to the database. The prefix keeps keys non-empty.
        private static string ToKey(string url) => "url:" + url.ToLowerInvariant();

        private bool IsEnabled() { return _settingsService.GetSettings().EnableBloomFilter; }
    }
}
