using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class SeoContentController : ManagementApiControllerBase
    {
        private readonly IContentService _contentService;
        private readonly SeoDisplayCollection _seoDisplayCollection;

        public SeoContentController(IContentService contentService, SeoDisplayCollection seoDisplayCollection)
        {
            _contentService = contentService;
            _seoDisplayCollection = seoDisplayCollection;
        }

        [HttpGet("seoContent")]
        public IActionResult Get(int contentId)
        {
            var content = _contentService.GetById(contentId);
            return new JsonResult(new SeoContentViewModel
            {
                Displays = _seoDisplayCollection.Select(it => it.Get(content)).WhereNotNull().ToArray()
            });
        }
    }
}
