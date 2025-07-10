using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    [BackOfficeRoute("seoToolkit/tree/info")]
    public class SeoToolkitTreeController : SeoToolkitControllerBase
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;
        private readonly SeoTreeSectionCollection _seoTreeSections;

        private Guid _domainGuid = new Guid("ab248b43-9757-432a-9821-22f9eeb513e7");

        public SeoToolkitTreeController(SeoTreeSectionCollection seoTreeSections)
        {
            _seoTreeSections = seoTreeSections;
        }

        [HttpGet("root")]
        [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>> GetRoot(int skip = 0, int take = 100)
        {
            var hasDomainSpecific = _seoTreeSections.Any(it => it.CanBeDomainSpecific);
            var items = _seoTreeSections.Select(it => new NamedEntityTreeItemResponseModel
            {
                Id = it.Id,
                Name = it.Name
            }).ToList();
            if (hasDomainSpecific)
            {
                items.Add(new NamedEntityTreeItemResponseModel
                {
                    Id = _domainGuid,
                    Name = "Domains"
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
            var result = new PagedViewModel<NamedEntityTreeItemResponseModel>()
            {
                Items = items,
                Total = items.Count
            };

            return Ok(result);
        }

        [HttpGet("children")]
        [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>> GetChildren(Guid parentId, int skip = 0, int take = 100)
        {
            var result = new PagedViewModel<NamedEntityTreeItemResponseModel>()
            {
                Items = [],
                Total = 0
            };

            return Ok(result);
        }

        [HttpGet("ancestors")]
        [ProducesResponseType(typeof(IEnumerable<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<NamedEntityTreeItemResponseModel>> GetAncestors(Guid descendantId)
        {
            return Ok(Enumerable.Empty<NamedEntityTreeItemResponseModel>());
        }
    }
}
