using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Deploy.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.Deploy.Controllers
{
    /// <summary>
    /// Backoffice endpoint that tells the Deploy client which per-node SEO entities exist for a
    /// content node, so the Transfer/Queue actions only move the entities that have data.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit Deploy")]
    [BackOfficeRoute("seoToolkitDeploy")]
    public class SeoToolkitDeployController(
        IMetaFieldsValueRepository valueRepository,
        ISitemapService sitemapService)
        : SeoToolkitAuthenticatedControllerBase
    {
        [HttpGet("seoTransferItems")]
        [ProducesResponseType(typeof(SeoTransferItemsViewModel), 200)]
        public IActionResult GetSeoTransferItems(Guid contentKey)
        {
            var items = new List<SeoTransferItem>();

            if (valueRepository.HasAnyValues(contentKey))
            {
                items.Add(new SeoTransferItem(contentKey.ToString(), SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue));
            }

            if (sitemapService.GetContentSettings(contentKey) is not null)
            {
                items.Add(new SeoTransferItem(contentKey.ToString(), SeoToolkitDeployConstants.UdiEntityType.SitemapContent));
            }

            return Ok(new SeoTransferItemsViewModel { Items = items });
        }
    }
}
