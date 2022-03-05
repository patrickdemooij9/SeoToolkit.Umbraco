using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Scoping;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Database;

namespace uSeoToolkit.Umbraco.Redirects.Core.Repositories
{
    public class RedirectsRepository : IRedirectsRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public RedirectsRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Save(Redirect redirect)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.Database.Save(ToEntity(redirect));
            }
        }

        public IEnumerable<Redirect> GetAll()
        {
            throw new System.NotImplementedException();
        }

        private RedirectEntity ToEntity(Redirect redirect)
        {
            return new RedirectEntity
            {
                Id = redirect.Id,
                Domain = redirect.Domain,
                CustomDomain = redirect.CustomDomain,
                IsRegex = redirect.IsRegex,
                OldUrl = redirect.OldUrl,
                NewNodeId = redirect.NewNode?.Id,
                NewUrl = redirect.NewUrl,
                LastUpdated = DateTime.Now,
                Notes = redirect.Notes,
                RedirectCode = redirect.RedirectCode
            };
        }
    }
}
