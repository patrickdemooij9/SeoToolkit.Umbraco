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
