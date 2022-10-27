using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Hubs
{
    public class SiteAuditHubClientService
    {
        private readonly IHubContext<SiteAuditHub> _hubContext;

        private readonly Dictionary<string, SiteAuditHubClient> _assignedSiteAudits;

        public SiteAuditHubClientService(IHubContext<SiteAuditHub> hubContext)
        {
            _hubContext = hubContext;
            _assignedSiteAudits = new Dictionary<string, SiteAuditHubClient>();
        }

        public void AssignClient(string clientId, int auditId)
        {
            if (_assignedSiteAudits.ContainsKey(clientId))
                _assignedSiteAudits[clientId] = new SiteAuditHubClient(clientId, auditId);
            else
                _assignedSiteAudits.Add(clientId, new SiteAuditHubClient(clientId, auditId));
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
            var clients = _assignedSiteAudits.Where(it => it.Value.AuditId == siteAudit.Id).ToArray();
            if (!clients.Any()) return;
            
            _hubContext?.Clients.Clients(clients.Select(it => it.Key).ToList()).SendAsync("update", new SiteAuditDetailViewModel(siteAudit));

            if (siteAudit.Status == SiteAuditStatus.Finished)
            {
                foreach (var client in clients)
                {
                    RemoveClient(client.Key);
                }
            }
        }

        //Cleanup any old connections older than an hour
        public void Cleanup()
        {
            foreach(var client in _assignedSiteAudits.Where(it => it.Value.LastAccessed.AddHours(1) < DateTime.UtcNow))
            {
                RemoveClient(client.Key);
            }
        }
    }
}
