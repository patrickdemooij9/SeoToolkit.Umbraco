using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Deploy.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Core.Transfer.Queue;

namespace SeoToolkit.Umbraco.Deploy.Controllers
{
    /// <summary>
    /// Discovers a content node's per-node SEO entities (optionally including descendants) and
    /// queues them for transfer server-side — never the content node itself.
    /// </summary>
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit Deploy")]
    [BackOfficeRoute("seoToolkitDeploy")]
    public class SeoToolkitDeployController(
        IMetaFieldsValueRepository valueRepository,
        ISitemapService sitemapService,
        IContentService contentService,
        IServiceConnectorFactory serviceConnectorFactory,
        ITransferQueue transferQueue,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : SeoToolkitAuthenticatedControllerBase
    {
        private const int DescendantPageSize = 500;

        [HttpGet("seoTransferItems")]
        [ProducesResponseType(typeof(SeoTransferItemsViewModel), 200)]
        public IActionResult GetSeoTransferItems(Guid contentKey, bool includeDescendants = false)
        {
            var items = new List<SeoTransferItem>();

            foreach (var nodeKey in GetNodeKeys(contentKey, includeDescendants))
            {
                if (valueRepository.HasAnyValues(nodeKey))
                {
                    items.Add(new SeoTransferItem(nodeKey.ToString(),
                        SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue));
                }

                if (sitemapService.GetContentSettings(nodeKey) is not null)
                {
                    items.Add(new SeoTransferItem(nodeKey.ToString(),
                        SeoToolkitDeployConstants.UdiEntityType.SitemapContent));
                }
            }

            return Ok(new SeoTransferItemsViewModel { Items = items });
        }

        /// <summary>
        /// Queues the node's SEO entities (and every descendant's when <paramref name="includeDescendants"/>
        /// is set) to Deploy's transfer queue in a single request. The content node is never queued.
        /// </summary>
        [HttpPost("seoQueueAdd")]
        [ProducesResponseType(typeof(SeoQueueAddResult), 200)]
        public async Task<IActionResult> AddSeoToQueue(
            Guid contentKey,
            bool includeDescendants = false,
            DateTime? releaseDate = null,
            CancellationToken cancellationToken = default)
        {
            // The transfer queue is per-user; queue against the signed-in backoffice user.
            var userId = backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id;
            if (userId is null)
            {
                return Unauthorized();
            }

            var added = 0;
            foreach (var nodeKey in GetNodeKeys(contentKey, includeDescendants))
            {
                if (valueRepository.HasAnyValues(nodeKey))
                {
                    added += await QueueSeoEntityAsync(
                        SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue,
                        nodeKey, userId.Value, releaseDate, cancellationToken).ConfigureAwait(false);
                }

                if (sitemapService.GetContentSettings(nodeKey) is not null)
                {
                    added += await QueueSeoEntityAsync(
                        SeoToolkitDeployConstants.UdiEntityType.SitemapContent,
                        nodeKey, userId.Value, releaseDate, cancellationToken).ConfigureAwait(false);
                }
            }

            return Ok(new SeoQueueAddResult { Added = added });
        }

        // Resolves the SEO entity's UDI range via its Deploy connector and queues it (selector
        // "this"). Returns 1 when the queue accepted the item (added or replaced), else 0.
        private async Task<int> QueueSeoEntityAsync(
            string entityType, Guid nodeKey, int userId, DateTime? releaseDate, CancellationToken cancellationToken)
        {
            var connector = serviceConnectorFactory.GetConnector(entityType);
            var range = await connector.GetRangeAsync(entityType, nodeKey.ToString(), "this", cancellationToken)
                .ConfigureAwait(false);

            return transferQueue.Add(userId, new QueueItem(range, "*", releaseDate)) ? 1 : 0;
        }

        // The node itself, plus every descendant at any depth when requested (paged by the node's
        // integer id, as GetPagedDescendants is id-based).
        private IEnumerable<Guid> GetNodeKeys(Guid contentKey, bool includeDescendants)
        {
            yield return contentKey;

            if (!includeDescendants)
            {
                yield break;
            }

            var root = contentService.GetById(contentKey);
            if (root is null)
            {
                yield break;
            }

            var page = 0L;
            long total;
            do
            {
                var descendants =
                    contentService.GetPagedDescendants(root.Id, page, DescendantPageSize, out total);
                foreach (var descendant in descendants)
                {
                    yield return descendant.Key;
                }

                page++;
            }
            while (page * DescendantPageSize < total);
        }
    }
}
