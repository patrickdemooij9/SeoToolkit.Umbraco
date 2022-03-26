using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
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
        private const string BaseCacheKey = "redirects_";

        private readonly IScopeProvider _scopeProvider;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILocalizationService _localizationService;
        private readonly AppCaches _appCaches;

        public RedirectsRepository(IScopeProvider scopeProvider,
            IUmbracoContextFactory umbracoContextFactory,
            ILocalizationService localizationService,
            AppCaches appCaches)
        {
            _scopeProvider = scopeProvider;
            _umbracoContextFactory = umbracoContextFactory;
            _localizationService = localizationService;
            _appCaches = appCaches;
        }

        public void Save(Redirect redirect)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Save(ToEntity(redirect));
            }

            ClearCache();
        }

        public void Delete(Redirect redirect)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                scope.Database.Delete(ToEntity(redirect));
            }

            ClearCache();
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

        public IEnumerable<Redirect> GetAll(int pageNumber, int pageSize, out long totalRecords)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedirectEntity>()
                    .OrderBy<RedirectEntity>(it => it.Id);

                var result = scope.Database.Page<RedirectEntity>(pageNumber, pageSize, sql);
                totalRecords = result.TotalItems;
                return result.Items.Select(ToModel);
            }
        }

        public IEnumerable<Redirect> GetAllRegexRedirects()
        {
            return _appCaches.RuntimeCache.GetCacheItem($"{BaseCacheKey}GetRegexRedirects", () =>
            {
                using (var scope = _scopeProvider.CreateScope(autoComplete: true))
                {
                    return scope.Database.Fetch<RedirectEntity>(scope.SqlContext.Sql()
                            .SelectAll()
                            .From<RedirectEntity>()
                            .Where<RedirectEntity>(it => it.IsRegex))
                        .Select(ToModel).ToArray();
                }
            }, TimeSpan.FromMinutes(10));
        }

        public IEnumerable<Redirect> GetByUrls(params string[] paths)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                return scope.Database.Fetch<RedirectEntity>(scope.SqlContext.Sql()
                        .SelectAll()
                        .From<RedirectEntity>()
                        .Where<RedirectEntity>(it => !it.IsRegex && paths.Contains(it.OldUrl)))
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

        private void ClearCache()
        {
            _appCaches.RuntimeCache.ClearByKey(BaseCacheKey);
        }
    }
}
