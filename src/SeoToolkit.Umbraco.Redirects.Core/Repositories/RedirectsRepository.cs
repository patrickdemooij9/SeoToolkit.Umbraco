using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.Redirects.Core.Repositories
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

        public IEnumerable<Redirect> GetAll(int pageNumber, int pageSize, out long totalRecords, string orderBy = null, string orderDirection = null, string search = "")
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectAll()
                    .From<RedirectEntity>();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    sql = sql.Where<RedirectEntity>(it => it.OldUrl.Contains(search) ||
                                                          it.NewUrl.Contains(search) ||
                                                          it.CustomDomain.Contains(search));
                }

                // Translate alternative names (from list view) to the correct columns
                var orderingColumn = GetOrderingColumn(orderBy);
                
                // Translate the input to the correct ordering direction. By default ascending is used.
                sql = "desc".InvariantEquals(orderDirection)
                    ? sql.OrderByDescending(orderingColumn)
                    : sql.OrderBy<RedirectEntity>(orderingColumn);

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
                CreatedBy = redirect.CreatedBy,
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
                    CreatedBy = entity.CreatedBy,
                    RedirectCode = entity.RedirectCode
                };
            }
        }

        private void ClearCache()
        {
            _appCaches.RuntimeCache.ClearByKey(BaseCacheKey);
        }

        private Expression<Func<RedirectEntity, object>> GetOrderingColumn(string orderBy)
        {
            if ("name".InvariantEquals(orderBy) || "from".InvariantEquals(orderBy))
                return entity => entity.OldUrl;
            
            if ("to".InvariantEquals(orderBy))
                return entity => entity.NewUrl;
            
            if ("statusCode".InvariantEquals(orderBy))
                return entity => entity.RedirectCode;

            if ("lastUpdated".InvariantEquals(orderBy))
                return entity => entity.LastUpdated;

            return entity => entity.Id;
        }
    }
}
