using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    [PluginController("SeoToolkit")]
    public class SeoContentController : UmbracoAuthorizedApiController
    {
        private readonly IContentService _contentService;
        private readonly SeoDisplayCollection _seoDisplayCollection;

        public SeoContentController(IContentService contentService, SeoDisplayCollection seoDisplayCollection)
        {
            _contentService = contentService;
            _seoDisplayCollection = seoDisplayCollection;
        }

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
