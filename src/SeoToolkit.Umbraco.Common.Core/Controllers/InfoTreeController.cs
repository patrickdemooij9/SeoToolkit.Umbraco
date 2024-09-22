using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Constants;
using System;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    //This controller is only here to prevent single node trees if you only download one package
    [ApiExplorerSettings(GroupName = "seoToolkit")]
    [BackOfficeRoute("seoToolkit/tree/info")]
    public class InfoTreeController : SeoToolkitControllerBase
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        [HttpGet("root")]
        [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
        public ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>> GetRoot(int skip = 0, int take = 100)
        {
            var items = new[] { new NamedEntityTreeItemResponseModel 
            {
                Id = Guid.NewGuid(),
                Name = "Info",
                HasChildren = false
            }};
            var result = new PagedViewModel<NamedEntityTreeItemResponseModel>()
            {
                Items = items,
                Total = 1
            };

            return Ok(result);
        }
    }
}
