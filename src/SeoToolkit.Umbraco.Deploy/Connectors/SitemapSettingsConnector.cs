using SeoToolkit.Umbraco.Deploy.ArtifactModels;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;
using static Umbraco.Cms.Core.Constants;
using static Umbraco.Cms.Core.Constants.Conventions;

namespace SeoToolkit.Umbraco.Deploy.Connectors
{
    [UdiDefinition("sitemap-settings", UdiType.GuidUdi)]
    internal class SitemapSettingsConnector : ServiceConnectorBase<SitemapSettingsArtifact, GuidUdi, ArtifactDeployState<SitemapSettingsArtifact, SitemapPageSettings>>
    {
        private readonly ISitemapService _sitemapService;

        protected override string[] ValidOpenSelectors => [DeploySelector.This];

        protected override string OpenUdiName => "Sitemap settings";

        protected override int[] ProcessPasses => [2];

        public SitemapSettingsConnector(ISitemapService sitemapService)
        {
            _sitemapService = sitemapService;
        }

        public override async IAsyncEnumerable<GuidUdi> ExpandRangeAsync(UdiRange range, CancellationToken cancellationToken = default)
        {
            if (range.Udi.IsRoot)
            {
                foreach (var item in _sitemapService.GetAll())
                {
                    yield return new GuidUdi("sitemap-settings", item.ContentTypeGuid);
                }
                yield break;
            }

            var entity = _sitemapService.GetPageTypeSettings(((GuidUdi)range.Udi).Guid);
            if (entity == null)
            {
                yield break;
            }

            yield return new GuidUdi("sitemap-settings", entity.ContentTypeGuid);
        }

        public override Task<SitemapSettingsArtifact?> GetArtifactAsync(GuidUdi udi, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            var item = _sitemapService.GetPageTypeSettings(udi.Guid);
            if (item is null)
                return Task.FromResult<SitemapSettingsArtifact?>(null);
            return Task.FromResult<SitemapSettingsArtifact?>(new SitemapSettingsArtifact(new GuidUdi("sitemap-settings", item.ContentTypeGuid))
            {
                HideFromSitemap = item.HideFromSitemap,
                ChangeFrequency = item.ChangeFrequency,
                Priority = item.Priority
            });
        }

        public override Task<SitemapSettingsArtifact> GetArtifactAsync(ArtifactDeployState<SitemapSettingsArtifact, SitemapPageSettings> entity, IContextCache contextCache, CancellationToken cancellationToken = default)
        {
            if (entity.Entity is null)
                return Task.FromResult<SitemapSettingsArtifact>(null);

            return Task.FromResult(new SitemapSettingsArtifact(new GuidUdi("sitemap-settings", entity.Entity.ContentTypeGuid))
            {
                HideFromSitemap = entity.Entity.HideFromSitemap,
                ChangeFrequency = entity.Entity.ChangeFrequency,
                Priority = entity.Entity.Priority
            });
        }

        public override Task<NamedUdiRange> GetRangeAsync(GuidUdi udi, string selector, CancellationToken cancellationToken = default)
        {
            if (udi.IsRoot)
            {
                return Task.FromResult(new NamedUdiRange(udi, OpenUdiName, selector));
            }
            //TODO: Use IContentTypeService
            return Task.FromResult(new NamedUdiRange(udi, _sitemapService.GetPageTypeSettings(udi.Guid).ContentTypeGuid.ToString(), selector))
        }

        public override Task<NamedUdiRange> GetRangeAsync(string entityType, string sid, string selector, CancellationToken cancellationToken = default)
        {
            if (Guid.TryParse(sid, out var guid))
            {

            }
        }

        public override Task ProcessAsync(ArtifactDeployState<SitemapSettingsArtifact, ArtifactDeployState<SitemapSettingsArtifact, SitemapPageSettings>> state, IDeployContext context, int pass, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ArtifactDeployState<SitemapSettingsArtifact, ArtifactDeployState<SitemapSettingsArtifact, SitemapPageSettings>>> ProcessInitAsync(SitemapSettingsArtifact artifact, IDeployContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
