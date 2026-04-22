using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ExternalSiteAuditClient> _logger;

        public ExternalSiteAuditClient(HttpClient httpClient, IOptionsMonitor<SiteAuditAppSettingsModel> appSettings, ILogger<ExternalSiteAuditClient> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _logger = logger;
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
                _logger.LogWarning("External site audit start failed with status code {StatusCode}", response.StatusCode);
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
                _logger.LogWarning("External site audit detail fetch failed for {AuditId} with status code {StatusCode}", auditId, response.StatusCode);
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

            var response = await _httpClient.PostAsync(endpoint, null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External site audit stop failed for {AuditId} with status code {StatusCode}", auditId, response.StatusCode);
            }
        }

        private string FormatEndpoint(string endpoint, Guid auditId)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return null;
            }

            const string placeholder = "{id}";
            var firstPlaceholderIndex = endpoint.IndexOf(placeholder, StringComparison.Ordinal);
            if (firstPlaceholderIndex < 0)
            {
                _logger.LogWarning("External site audit endpoint does not contain {Placeholder}", placeholder);
                return null;
            }

            if (firstPlaceholderIndex != endpoint.LastIndexOf(placeholder, StringComparison.Ordinal))
            {
                _logger.LogWarning("External site audit endpoint contains placeholder {Placeholder} more than once", placeholder);
                return null;
            }

            return endpoint.Replace(placeholder, Uri.EscapeDataString(auditId.ToString()), StringComparison.Ordinal);
        }
    }
}
