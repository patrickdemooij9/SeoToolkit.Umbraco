using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.SiteAudit.Core.Enums;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Repositories
{
    public class SiteAuditDatabaseRepository : ISiteAuditRepository
    {
        private readonly IScopeAccessor _scopeAccessor;
        private readonly ISiteCheckService _siteCheckService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        private IScope AmbientScope
        {
            get
            {
                if (_scopeAccessor.AmbientScope is null)
                    throw new Exception("No scope!");
                return _scopeAccessor.AmbientScope;
            }
        }

        private IUmbracoDatabase Database => AmbientScope.Database;

        public SiteAuditDatabaseRepository(IScopeAccessor scopeAccessor, ISiteCheckService siteCheckService, IUmbracoContextFactory umbracoContextFactory)
        {
            _scopeAccessor = scopeAccessor;
            _siteCheckService = siteCheckService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public SiteAuditDto Add(SiteAuditDto model)
        {
            return Update(model);
        }

        public void Delete(int id)
        {
            foreach (var page in Database.Fetch<SiteAuditPageEntity>(AmbientScope.SqlContext.Sql().SelectAll()
                .From<SiteAuditPageEntity>()
                .Where<SiteAuditPageEntity>(it => it.AuditId == id)))
            {
                foreach (var result in Database.Fetch<SiteAuditCheckResultEntity>(AmbientScope.SqlContext.Sql()
                    .SelectAll()
                    .From<SiteAuditCheckResultEntity>()
                    .Where<SiteAuditCheckResultEntity>(it => it.PageId == page.Id)))
                {
                    Database.Delete(result);
                }
                Database.Delete(page);
            }
            foreach (var check in Database.Fetch<SiteAuditCheckEntity>(AmbientScope.SqlContext.Sql()
                .SelectAll()
                .From<SiteAuditCheckEntity>()
                .Where<SiteAuditCheckEntity>(it => it.AuditId == id)))
            {
                Database.Delete(check);
            }

            Database.Delete<SiteAuditEntity>(id);
        }

        public void SaveCrawledPage(SiteAuditDto audit, CrawledPageDto page)
        {
            var pageEntity = new SiteAuditPageEntity
            {
                AuditId = audit.Id,
                StatusCode = page.StatusCode,
                Url = page.PageUrl.ToString()
            };
            Database.Insert(pageEntity);

            foreach (var result in page.Results)
            {
                Database.Insert(new SiteAuditCheckResultEntity
                {
                    PageId = pageEntity.Id,
                    CheckId = result.Check.Id,
                    ResultId = (int)result.Result,
                    ExtraValues = JsonConvert.SerializeObject(result.ExtraValues)
                });
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
                var entities = Database.Fetch<SiteAuditEntity>();
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
                        StartingUrl = new Uri(entity.StartingUrl),
                        SiteChecks = Database.Fetch<SiteAuditCheckEntity>(AmbientScope.SqlContext.Sql()
                                .SelectAll()
                                .From<SiteAuditCheckEntity>()
                                .Where<SiteAuditCheckEntity>(it => it.AuditId == entity.Id))
                            .Select(it => _siteCheckService.GetAll().FirstOrDefault(s => s.Id == it.CheckId)).ToList(),
                        CrawledPages = new ConcurrentQueue<CrawledPageDto>(Database.Fetch<SiteAuditPageEntity>(AmbientScope.SqlContext.Sql()
                                .SelectAll()
                                .From<SiteAuditPageEntity>()
                                .Where<SiteAuditPageEntity>(it => it.AuditId == entity.Id)).Select(it => Map(AmbientScope, it))
                            .ToArray())
                    };
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
                    StartingUrl = model.StartingUrl.ToString(),
                    MaxPagesToCrawl = model.MaxPagesToCrawl,
                    DelayBetweenRequests = model.DelayBetweenRequests,
                    TotalPagesFound = model.TotalPagesFound
                };

                if (isNew)
                    Database.Insert(entity);
                else
                    Database.Update(entity);
                //TODO: Probably save pages and results here again?

                var currentSiteChecks = GetChecksByAudit(entity.Id).ToArray();
                foreach (var newCheck in model.SiteChecks.Where(it => currentSiteChecks.All(x => x.CheckId != it.Id)))
                {
                    var checkEntity = new SiteAuditCheckEntity { AuditId = entity.Id, CheckId = newCheck.Id };
                    Database.Insert(checkEntity);
                }

                foreach (var deletedCheck in currentSiteChecks.Where(it =>
                    model.SiteChecks.All(x => x.Id != it.CheckId)))
                {
                    Database.Delete(deletedCheck);
                }

                model.Id = entity.Id;
                return model;
            }
        }

        private IEnumerable<SiteAuditCheckEntity> GetChecksByAudit(int auditId)
        {
            return Database.Fetch<SiteAuditCheckEntity>(AmbientScope.SqlContext.Sql()
                .SelectAll()
                .From<SiteAuditCheckEntity>()
                .Where<SiteAuditCheckEntity>(it => it.AuditId == auditId));
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
