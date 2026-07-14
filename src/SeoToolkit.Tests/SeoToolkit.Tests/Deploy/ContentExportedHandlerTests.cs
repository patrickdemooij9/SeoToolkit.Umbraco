using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Events;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class ContentExportedHandlerTests
    {
        private Mock<IMetaFieldsValueRepository> _valueRepository = null!;
        private Mock<ISitemapService> _sitemapService = null!;
        private SeoToolkitContentExportedHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _valueRepository = new Mock<IMetaFieldsValueRepository>();
            _valueRepository.Setup(r => r.HasAnyValues(It.IsAny<Guid>())).Returns(false);
            _sitemapService = new Mock<ISitemapService>();
            _handler = new SeoToolkitContentExportedHandler(_valueRepository.Object, _sitemapService.Object);
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

            var dependencyUdis = artifact.Dependencies.Select(d => d.Udi).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(dependencyUdis,
                    Does.Contain(new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, nodeKey)));
                Assert.That(dependencyUdis,
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
