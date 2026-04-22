using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SeoToolkit.Umbraco.SiteAudit.Core.Config.Models;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public class ExternalSiteAuditClient : IExternalSiteAuditClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsMonitor<SiteAuditAppSettingsModel> _appSettings;

        public ExternalSiteAuditClient(HttpClient httpClient, IOptionsMonitor<SiteAuditAppSettingsModel> appSettings)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
        }

        public bool IsEnabled()
        {
            var settings = _appSettings.CurrentValue;
            return settings.UseExternalCrawling
                && !string.IsNullOrWhiteSpace(settings.ExternalCrawlRequestEndpoint)
                && !string.IsNullOrWhiteSpace(settings.ExternalAuditDetailEndpoint);
        }

        public async Task<Guid?> StartAudit(ExternalSiteAuditCreateRequestDto request)
        {
            if (!IsEnabled())
            {
                return null;
            }

            var settings = _appSettings.CurrentValue;
            var response = await _httpClient.PostAsJsonAsync(settings.ExternalCrawlRequestEndpoint, request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ExternalSiteAuditCreateResponseDto>();
            return result?.AuditId;
        }

        public async Task<SiteAuditDetailViewModel> GetAuditDetail(Guid auditId)
        {
            if (!IsEnabled())
            {
                return null;
            }

            var settings = _appSettings.CurrentValue;
            var endpoint = FormatEndpoint(settings.ExternalAuditDetailEndpoint, auditId);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return null;
            }

            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SiteAuditDetailViewModel>();
        }

        public async Task StopAudit(Guid auditId)
        {
            if (!IsEnabled())
            {
                return;
            }

            var settings = _appSettings.CurrentValue;
            if (string.IsNullOrWhiteSpace(settings.ExternalStopAuditEndpoint))
            {
                return;
            }

            var endpoint = FormatEndpoint(settings.ExternalStopAuditEndpoint, auditId);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return;
            }

            await _httpClient.PostAsync(endpoint, null);
        }

        private string FormatEndpoint(string endpoint, Guid auditId)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return null;
            }

            return endpoint.Replace("{id}", Uri.EscapeDataString(auditId.ToString()));
        }
    }
}
