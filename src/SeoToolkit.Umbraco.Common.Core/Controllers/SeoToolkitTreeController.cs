using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    //This controller is only here to prevent single node trees if you only download one package
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    [BackOfficeRoute("seoToolkit/tree/info")]
    public class SeoToolkitTreeController : SeoToolkitControllerBase
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        private Guid _infoGuid = new Guid("CDF429D1-2380-4AC2-AC3E-22D619EE4529");
        private Guid _robotsGuid = new Guid("20A2086E-7D72-44BA-B97B-5836CAF6E28E");
        private Guid _scriptManagerGuid = new Guid("94E95F4A-2ECB-4038-BCFD-8357B7C41F1A");
        private Guid _redirectsGuid = new Guid("1147F58D-D2D5-425B-AEDE-DB537BDAC9EF");
        private Guid _siteAuditGuid = new Guid("B0D1C655-472B-40E7-9AC4-C6328EA9CF32");
        private Guid _notFoundGuid = new Guid("a9b6dec6-e045-476a-ba3f-742355e18e33");

        [HttpGet("root")]
        [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>> GetRoot(int skip = 0, int take = 100)
        {
            var items = new[] { new NamedEntityTreeItemResponseModel
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
            } };
            var result = new PagedViewModel<NamedEntityTreeItemResponseModel>()
            {
                Items = items,
                Total = items.Length
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
