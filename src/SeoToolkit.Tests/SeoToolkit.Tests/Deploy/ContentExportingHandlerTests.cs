using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Events;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ContentExportingHandlerTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<ISitemapService> _sitemapService = null!;
        private SeoToolkitDeploySettings _settings = null!;
        private SeoToolkitContentExportingHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _valueRepository.Setup(r => r.HasAnyValues(It.IsAny<Guid>())).Returns(false);
            _sitemapService = new Mock<ISitemapService>();
            _settings = new SeoToolkitDeploySettings();
            var settingsMonitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            settingsMonitor.Setup(m => m.CurrentValue).Returns(() => _settings);
            _handler = new SeoToolkitContentExportingHandler(
                _valueRepository.Object, _sitemapService.Object, settingsMonitor.Object);
        }

        private static ArtifactExportingNotification Notify(DocumentArtifact artifact)
            => new(artifact, new EventMessages());

        [Test]
        public async Task DocumentWithSeoData_GetsBothDependenciesAppended()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = new DocumentArtifact(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)) { Name = "Page" };
            _valueRepository.Setup(r => r.HasAnyValues(nodeKey)).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(nodeKey))
                .Returns(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });

            await _handler.HandleAsync(Notify(artifact), CancellationToken.None);

            var seoDependencies = artifact.Dependencies
                .Where(d => d.Udi.EntityType.StartsWith("seotoolkit-"))
                .ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(seoDependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey)));
                Assert.That(seoDependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, nodeKey)));
                // Match mode so changed SEO data is re-transferred, not just ensured-present.
                Assert.That(seoDependencies.Select(d => d.Mode),
                    Has.All.EqualTo(ArtifactDependencyMode.Match));
            });
        }

        [Test]
        public async Task DisabledEntityType_DependencyIsNotAppended()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = new DocumentArtifact(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)) { Name = "Page" };
            _valueRepository.Setup(r => r.HasAnyValues(nodeKey)).Returns(true);
            _sitemapService.Setup(s => s.GetContentSettings(nodeKey))
                .Returns(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });
            // MetaFieldsValue connector disabled: it returns no artifact, so a Match dependency on
            // it can never be satisfied and must not be appended.
            _settings.DisabledEntityTypes = [SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue];

            await _handler.HandleAsync(Notify(artifact), CancellationToken.None);

            var seoDependencies = artifact.Dependencies.Select(d => d.Udi).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(seoDependencies,
                    Does.Not.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey)));
                // Sitemap connector is still enabled, so its dependency is still appended.
                Assert.That(seoDependencies,
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, nodeKey)));
            });
        }

        [Test]
        public async Task DocumentWithoutSeoData_IsUntouched()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = new DocumentArtifact(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)) { Name = "Page" };
            _valueRepository.Setup(r => r.HasAnyValues(nodeKey)).Returns(false);
            _sitemapService.Setup(s => s.GetContentSettings(nodeKey)).Returns((SitemapContentSettings?)null);

            await _handler.HandleAsync(Notify(artifact), CancellationToken.None);

            Assert.That(artifact.Dependencies.Select(d => d.Udi.EntityType),
                Has.None.StartsWith("seotoolkit-"));
        }
    }
}
