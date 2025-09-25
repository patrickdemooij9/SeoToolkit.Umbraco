using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit")]
    [BackOfficeRoute("seoToolkit/tree/info")]
    public class SeoToolkitTreeController : SeoToolkitAuthenticatedControllerBase
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;
        private readonly SeoTreeSectionCollection _seoTreeSections;
        private readonly ISeoDomainsService _seoDomainsService;
        private Guid _domainGuid = new Guid("ab248b43-9757-432a-9821-22f9eeb513e7");

        public SeoToolkitTreeController(SeoTreeSectionCollection seoTreeSections, ISeoDomainsService seoDomainsService)
        {
            _seoTreeSections = seoTreeSections;
            _seoDomainsService = seoDomainsService;
        }

        [HttpGet("root")]
        [ProducesResponseType(typeof(PagedViewModel<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<SeoToolkitTreeItemApiModel>> GetRoot(int skip = 0, int take = 100)
        {
            var hasDomainSpecific = _seoTreeSections.Any(it => it.CanBeDomainSpecific);
            var items = _seoTreeSections.Select(it => new SeoToolkitTreeItemApiModel
            {
                Id = it.Id.ToString(),
                Name = it.Name
            }).ToList();
            if (hasDomainSpecific)
            {
                items.Add(new SeoToolkitTreeItemApiModel
                {
                    Id = _domainGuid.ToString(),
                    Name = "Domains",
                    HasChildren = _seoDomainsService.GetAll().Length > 0
                });
            }

            /*var items = new[] { new NamedEntityTreeItemResponseModel
            {
                Id = _infoGuid,
                Name = "Info",
            }, new NamedEntityTreeItemResponseModel{
                Id = _robotsGuid,
                Name = "Robots.txt",
            }, new NamedEntityTreeItemResponseModel{
                Id = _scriptManagerGuid,
                Name = "Script Manager"
            }, new NamedEntityTreeItemResponseModel{
                Id = _redirectsGuid,
                Name = "Redirects",
            }, new NamedEntityTreeItemResponseModel{
                Id = _siteAuditGuid,
                Name = "Site Audits"
            }, new NamedEntityTreeItemResponseModel{
                Id = _notFoundGuid,
                Name = "Not Found"
            } };*/
            var result = new PagedViewModel<SeoToolkitTreeItemApiModel>()
            {
                Items = items,
                Total = items.Count
            };

            return Ok(result);
        }

        [HttpGet("children")]
        [ProducesResponseType(typeof(PagedViewModel<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<SeoToolkitTreeItemApiModel>> GetChildren(string parentUnique, int skip = 0, int take = 100)
        {
            if (Guid.TryParse(parentUnique, out var resultGuid) && resultGuid == _domainGuid)
            {
                var allItems = _seoDomainsService.GetAll();
                var items = allItems.Skip(skip).Take(take).Select(it => new SeoToolkitTreeItemApiModel
                {
                    Id = $"{_domainGuid}~{it.Id}",
                    Name = it.Name,
                    HasChildren = GetSectionsForDomain(it.Id).Length > 0
                }).ToArray();
                return Ok(new PagedViewModel<SeoToolkitTreeItemApiModel>()
                {
                    Items = items,
                    Total = allItems.Length
                });
            }
            else if (parentUnique.StartsWith($"{_domainGuid}~"))
            {
                var domainId = int.Parse(parentUnique.Replace($"{_domainGuid}~", ""));
                var sections = GetSectionsForDomain(domainId);
                return new PagedViewModel<SeoToolkitTreeItemApiModel>
                {
                    Items = sections.Select(it => new SeoToolkitTreeItemApiModel
                    {
                        Id = $"{it.Id}~{domainId}".ToLower(),
                        Name = it.Name,
                        ParentId = parentUnique,
                    }).ToArray(),
                    Total = sections.Length
                };
            }

            var result = new PagedViewModel<SeoToolkitTreeItemApiModel>()
            {
                Items = [],
                Total = 0
            };

            return Ok(result);
        }

        [HttpGet("ancestors")]
        [ProducesResponseType(typeof(IEnumerable<SeoToolkitTreeItemApiModel>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<SeoToolkitTreeItemApiModel>> GetAncestors(Guid descendantId)
        {
            return Ok(Enumerable.Empty<SeoToolkitTreeItemApiModel>());
        }

        private ISeoTreeSection[] GetSectionsForDomain(int domainId)
        {
            var domain = _seoDomainsService.GetAll().FirstOrDefault(it => it.Id == domainId);
            if (domain is null) return [];
            return _seoTreeSections.Where(it => it.CanBeDomainSpecific && domain.Settings.ContainsKey($"Module.{it.Id}")).ToArray();
        }
    }
}
