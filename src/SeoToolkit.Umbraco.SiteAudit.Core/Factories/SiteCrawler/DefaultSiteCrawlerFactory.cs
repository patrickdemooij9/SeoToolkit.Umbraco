using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Config;
using SeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler;
using System.Net.Http;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Factories.SiteCrawler
{
    public class DefaultSiteCrawlerFactory : ISiteCrawlerFactory
    {
        private readonly ISettingsService<SiteAuditConfigModel> _settingsService;

        public DefaultSiteCrawlerFactory(ISettingsService<SiteAuditConfigModel> settingsService)
        {
            _settingsService = settingsService;
        }

        public ISiteCrawler CreateNew()
        {
            HttpClient httpClient = null;
            if (_settingsService.GetSettings().AllowInvalidCerts)
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                };
                httpClient = new HttpClient(httpClientHandler);
            }
            httpClient ??= new HttpClient();

            return new Core.SiteCrawler.SiteCrawler(new DefaultPageUrlRequester(httpClient), new DefaultScheduler(), new DefaultLinkParser());
        }
    }
}
