using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.NotFound.Core.Services;
using System;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.NotFound.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit NotFound")]
    [BackOfficeRoute("seoToolkitNotFound")]
    public class PageNotFoundController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly IPageNotFoundService _pageNotFoundService;

        public PageNotFoundController(IPageNotFoundService pageNotFoundService)
        {
            _pageNotFoundService = pageNotFoundService;
        }

        [HttpPost("notFound")]
        public void SetKeyValue(Guid? data, int? domainId)
        {
            _pageNotFoundService.SetPageNotFound(data, domainId);
        }

        [HttpGet("notFound")]
        [ProducesResponseType(typeof(Guid), 200)]
        public Guid? GetValue(int? domainId)
        {
            return _pageNotFoundService.GetPageNotFound(domainId);
        }
    }
}
