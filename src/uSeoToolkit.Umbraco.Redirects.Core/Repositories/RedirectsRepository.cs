using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Redirects.Core.Interfaces;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Business;
using uSeoToolkit.Umbraco.Redirects.Core.Models.Database;

namespace uSeoToolkit.Umbraco.Redirects.Core.Repositories
{
    public class RedirectsRepository : IRedirectsRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILocalizationService _localizationService;

        public RedirectsRepository(IScopeProvider scopeProvider,
            IUmbracoContextFactory umbracoContextFactory,
            ILocalizationService localizationService)
        {
            _scopeProvider = scopeProvider;
            _umbracoContextFactory = umbracoContextFactory;
            _localizationService = localizationService;
        }

        public void Save(Redirect redirect)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Save(ToEntity(redirect));
            }
        }

        public void Delete(Redirect redirect)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Delete(ToEntity(redirect));
            }
        }

        public Redirect Get(int id)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return ToModel(scope.Database.FirstOrDefault<RedirectEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedirectEntity>()
                    .Where<RedirectEntity>(it => it.Id == id)));
            }
        }

        public IEnumerable<Redirect> GetAll()
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return scope.Database.Fetch<RedirectEntity>(scope.SqlContext.Sql()
                        .SelectAll()
                        .From<RedirectEntity>())
                    .Select(ToModel);
            }
        }

        public IEnumerable<Redirect> GetByUrls(params string[] paths)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return scope.Database.Fetch<RedirectEntity>(scope.SqlContext.Sql()
                        .SelectAll()
                        .From<RedirectEntity>()
                        .Where<RedirectEntity>(it => paths.Contains(it.OldUrl)))
                    .Select(ToModel);
            }
        }

        private RedirectEntity ToEntity(Redirect redirect)
        {
            return new RedirectEntity
            {
                Id = redirect.Id,
                Domain = redirect.Domain?.Id,
                CustomDomain = redirect.CustomDomain,
                IsRegex = redirect.IsRegex,
                OldUrl = redirect.OldUrl,
                NewNodeId = redirect.NewNode?.Id,
                NewUrl = redirect.NewUrl,
                NewNodeCultureId = redirect.NewNodeCulture?.Id,
                LastUpdated = DateTime.Now,
                RedirectCode = redirect.RedirectCode
            };
        }

        private Redirect ToModel(RedirectEntity entity)
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                return new Redirect
                {
                    Id = entity.Id,
                    Domain = entity.Domain is null
                        ? null
                        : ctx.UmbracoContext.Domains.GetAll(false).FirstOrDefault(it => it.Id == entity.Domain),
                    CustomDomain = entity.CustomDomain,
                    IsRegex = entity.IsRegex,
                    OldUrl = entity.OldUrl,
                    NewNode = entity.NewNodeId is null
                        ? null
                        : entity.NewNodeCultureId is null ? ctx.UmbracoContext.Media.GetById(entity.NewNodeId.Value) : ctx.UmbracoContext.Content.GetById(entity.NewNodeId.Value),
                    NewNodeCulture = entity.NewNodeCultureId is null ? null : _localizationService.GetLanguageById(entity.NewNodeCultureId.Value),
                    NewUrl = entity.NewUrl,
                    LastUpdated = entity.LastUpdated,
                    RedirectCode = entity.RedirectCode
                };
            }
        }
    }
}
