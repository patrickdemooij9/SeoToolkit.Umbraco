using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.SiteAudit.Core.Enums;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Database;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Repositories
{
    public class SiteAuditDatabaseRepository : ISiteAuditRepository
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly ISiteCheckService _siteCheckService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SiteAuditDatabaseRepository(IScopeProvider scopeProvider, ISiteCheckService siteCheckService, IUmbracoContextFactory umbracoContextFactory)
        {
            _scopeProvider = scopeProvider;
            _siteCheckService = siteCheckService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public SiteAuditDto Add(SiteAuditDto model)
        {
            return Update(model);
        }

        public void Delete(int id)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                foreach (var page in scope.Database.Fetch<SiteAuditPageEntity>(scope.SqlContext.Sql().SelectAll()
                    .From<SiteAuditPageEntity>()
                    .Where<SiteAuditPageEntity>(it => it.AuditId == id)))
                {
                    foreach (var result in scope.Database.Fetch<SiteAuditCheckResultEntity>(scope.SqlContext.Sql()
                        .SelectAll()
                        .From<SiteAuditCheckResultEntity>()
                        .Where<SiteAuditCheckResultEntity>(it => it.PageId == page.Id)))
                    {
                        scope.Database.Delete(result);
                    }
                    scope.Database.Delete(page);
                }
                foreach (var check in scope.Database.Fetch<SiteAuditCheckEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SiteAuditCheckEntity>()
                    .Where<SiteAuditCheckEntity>(it => it.AuditId == id)))
                {
                    scope.Database.Delete(check);
                }

                scope.Database.Delete<SiteAuditEntity>(id);

                scope.Complete();
            }
        }

        public void SaveCrawledPage(SiteAuditDto audit, CrawledPageDto page)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var pageEntity = new SiteAuditPageEntity
                {
                    AuditId = audit.Id,
                    StatusCode = page.StatusCode,
                    Url = page.PageUrl.ToString()
                };
                scope.Database.Insert(pageEntity);
                scope.Complete();

                foreach (var result in page.Results)
                {
                    scope.Database.Insert(new SiteAuditCheckResultEntity
                    {
                        PageId = pageEntity.Id,
                        CheckId = result.Check.Id,
                        ResultId = (int)result.Result,
                        ExtraValues = JsonConvert.SerializeObject(result.ExtraValues)
                    });
                }
                scope.Complete();
            }
        }

        public SiteAuditDto Get(int id)
        {
            return GetAll().FirstOrDefault(it => it.Id == id);
        }

        public IEnumerable<SiteAuditDto> GetAll()
        {
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                using (var scope = _scopeProvider.CreateScope())
                {
                    var entities = scope.Database.Fetch<SiteAuditEntity>();
                    foreach (var entity in entities)
                    {
                        yield return new SiteAuditDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            CreatedDate = entity.CreatedDate,
                            Status = (SiteAuditStatus)entity.StatusId,
                            MaxPagesToCrawl = entity.MaxPagesToCrawl,
                            DelayBetweenRequests = entity.DelayBetweenRequests,
                            TotalPagesFound = entity.TotalPagesFound,
                            StartingUrl = new Uri(ctx.UmbracoContext.Content.GetById(entity.StartingNodeId)?.Url(mode: UrlMode.Absolute)),
                            SiteChecks = scope.Database.Fetch<SiteAuditCheckEntity>(scope.SqlContext.Sql()
                                .SelectAll()
                                .From<SiteAuditCheckEntity>()
                                .Where<SiteAuditCheckEntity>(it => it.AuditId == entity.Id))
                            .Select(it => _siteCheckService.GetAll().FirstOrDefault(s => s.Id == it.CheckId)).ToList(),
                            CrawledPages = new ConcurrentQueue<CrawledPageDto>(scope.Database.Fetch<SiteAuditPageEntity>(scope.SqlContext.Sql()
                                .SelectAll()
                                .From<SiteAuditPageEntity>()
                                .Where<SiteAuditPageEntity>(it => it.AuditId == entity.Id)).Select(it => Map(scope, it)))
                        };
                    }
                }
            }
        }

        public SiteAuditDto Update(SiteAuditDto model)
        {
            var isNew = model.Id == 0;
            using (var ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var entity = new SiteAuditEntity
                {
                    Id = isNew ? 0 : model.Id,
                    Name = model.Name,
                    CreatedDate = model.CreatedDate,
                    StatusId = (int)model.Status,
                    StartingNodeId = ctx.UmbracoContext.Content.GetByRoute(new Uri(model.StartingUrl.ToString()).AbsolutePath).Id,
                    MaxPagesToCrawl = model.MaxPagesToCrawl,
                    DelayBetweenRequests = model.DelayBetweenRequests,
                    TotalPagesFound = model.TotalPagesFound
                };
                using (var scope = _scopeProvider.CreateScope())
                {
                    if (isNew)
                        scope.Database.Insert(entity);
                    else
                        scope.Database.Update(entity);

                    scope.Complete();
                    //TODO: Probably save pages and results here again?
                }

                var currentSiteChecks = GetChecksByAudit(entity.Id).ToArray();
                using (var scope = _scopeProvider.CreateScope())
                {
                    foreach (var newCheck in model.SiteChecks.Where(it => currentSiteChecks.All(x => x.CheckId != it.Id)))
                    {
                        var checkEntity = new SiteAuditCheckEntity { AuditId = entity.Id, CheckId = newCheck.Id };
                        scope.Database.Insert(checkEntity);
                    }

                    foreach (var deletedCheck in currentSiteChecks.Where(it =>
                        model.SiteChecks.All(x => x.Id != it.CheckId)))
                    {
                        scope.Database.Delete(deletedCheck);
                    }

                    scope.Complete();
                }

                model.Id = entity.Id;
                return model;
            }
        }

        private IEnumerable<SiteAuditCheckEntity> GetChecksByAudit(int auditId)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                return scope.Database.Fetch<SiteAuditCheckEntity>(scope.SqlContext.Sql()
                    .SelectAll()
                    .From<SiteAuditCheckEntity>()
                    .Where<SiteAuditCheckEntity>(it => it.AuditId == auditId));
            }
        }

        //TODO: Move to mapper
        private CrawledPageDto Map(IScope scope, SiteAuditPageEntity entity)
        {
            var dto = new CrawledPageDto
            {
                PageUrl = new Uri(entity.Url),
                StatusCode = entity.StatusCode
            };
            foreach (var result in scope.Database.Fetch<SiteAuditCheckResultEntity>(scope.SqlContext.Sql()
                .SelectAll()
                .From<SiteAuditCheckResultEntity>()
                .Where<SiteAuditCheckResultEntity>(it => it.PageId == entity.Id)))
            {
                dto.Results.Add(new PageCrawlResult
                {
                    Check = _siteCheckService.GetCheckById(result.CheckId),
                    Result = (SiteCrawlResultType)result.ResultId,
                    ExtraValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.ExtraValues)
                });
            }

            return dto;
        }
    }
}
