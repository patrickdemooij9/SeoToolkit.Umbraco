using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Hubs
{
    public class SiteAuditHubClientService
    {
        private readonly IHubContext<SiteAuditHub> _hubContext;

        private readonly Dictionary<string, int> _assignedSiteAudits;

        public SiteAuditHubClientService(IHubContext<SiteAuditHub> hubContext)
        {
            _hubContext = hubContext;
            _assignedSiteAudits = new Dictionary<string, int>();
        }

        public void AssignClient(string clientId, int auditId)
        {
            if (_assignedSiteAudits.ContainsKey(clientId))
                _assignedSiteAudits[clientId] = auditId;
            else
                _assignedSiteAudits.Add(clientId, auditId);
        }

        public void RemoveClient(string clientId)
        {
            _assignedSiteAudits.Remove(clientId);
        }

        public void Update<T>(string clientId, T model)
        {
            if (_hubContext != null && !string.IsNullOrWhiteSpace(clientId))
            {
                var client = _hubContext.Clients.Client(clientId);
                if (client != null)
                {
                    client.SendAsync("update", model).RunSynchronously();
                }
            }
        }

        public void UpdateSiteAudit(SiteAuditDto siteAudit)
        {
            if (!_assignedSiteAudits.ContainsValue(siteAudit.Id))
                return;

            var clients = _assignedSiteAudits.Where(it => it.Value == siteAudit.Id);
            _hubContext?.Clients.Clients(clients.Select(it => it.Key).ToList()).SendAsync("update", new SiteAuditDetailViewModel(siteAudit));

            if (siteAudit.Status == SiteAuditStatus.Finished)
            {
                foreach (var client in _assignedSiteAudits.Where(it => it.Value == siteAudit.Id))
                {
                    RemoveClient(client.Key);
                }
            }
        }
    }
}
