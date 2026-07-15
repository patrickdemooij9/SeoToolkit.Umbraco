using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Disk;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class DiskRefresherHandlerTests
    {
        [Test]
        public async Task SeoSettingSaved_WritesArtifactViaConnector()
        {
            var contentTypeKey = Guid.NewGuid();
            var artifact = Mock.Of<IArtifact>();
            var connector = new Mock<IServiceConnector>();
            connector.Setup(c => c.GetArtifactAsync(
                    It.Is<Udi>(u => u.EntityType == SeoToolkitDeployConstants.UdiEntityType.SeoSetting),
                    It.IsAny<IContextCache>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(artifact);

            var factory = new Mock<IServiceConnectorFactory>();
            factory.Setup(f => f.GetConnector(SeoToolkitDeployConstants.UdiEntityType.SeoSetting))
                .Returns(connector.Object);
            var diskService = new Mock<IDiskEntityService>();

            var handler = new SeoSettingDiskRefresherHandler(diskService.Object, factory.Object);
            await handler.HandleAsync(new SeoSettingSavedNotification(contentTypeKey, true), CancellationToken.None);

            diskService.Verify(d => d.WriteArtifactsAsync(
                It.Is<IEnumerable<IArtifact>>(a => a.Contains(artifact)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ScriptDeleted_DeletesArtifactWithMatchingUdi()
        {
            var scriptKey = Guid.NewGuid();
            var factory = new Mock<IServiceConnectorFactory>();
            var diskService = new Mock<IDiskEntityService>();

            var handler = new ScriptDiskRefresherHandler(diskService.Object, factory.Object);
            await handler.HandleAsync(new ScriptDeletedNotification(scriptKey), CancellationToken.None);

            diskService.Verify(d => d.DeleteArtifacts(
                It.Is<IEnumerable<IArtifact>>(a => a.Any(x =>
                    x.Udi.EntityType == SeoToolkitDeployConstants.UdiEntityType.Script
                    && ((GuidUdi)x.Udi).Guid == scriptKey))), Times.Once);
        }

        [Test]
        public async Task MetaFieldsValueChanged_WithValues_WritesArtifact()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = Mock.Of<IArtifact>();
            var connector = new Mock<IServiceConnector>();
            connector.Setup(c => c.GetArtifactAsync(
                    It.Is<Udi>(u => u.EntityType == SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue),
                    It.IsAny<IContextCache>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(artifact);
            var factory = new Mock<IServiceConnectorFactory>();
            factory.Setup(f => f.GetConnector(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue))
                .Returns(connector.Object);
            var diskService = new Mock<IDiskEntityService>();
            var valueRepository = new Mock<IMetaFieldsValueRepository>();
            valueRepository.Setup(r => r.HasAnyValues(nodeKey)).Returns(true);

            var handler = new MetaFieldsValueDiskRefresherHandler(diskService.Object, factory.Object, valueRepository.Object);
            await handler.HandleAsync(new MetaFieldsValueChangedNotification(nodeKey), CancellationToken.None);

            diskService.Verify(d => d.WriteArtifactsAsync(
                It.Is<IEnumerable<IArtifact>>(a => a.Contains(artifact)), It.IsAny<CancellationToken>()), Times.Once);
            diskService.Verify(d => d.DeleteArtifacts(It.IsAny<IEnumerable<IArtifact>>()), Times.Never);
        }

        [Test]
        public async Task MetaFieldsValueChanged_NoValuesLeft_DeletesArtifact()
        {
            var nodeKey = Guid.NewGuid();
            var factory = new Mock<IServiceConnectorFactory>();
            var diskService = new Mock<IDiskEntityService>();
            var valueRepository = new Mock<IMetaFieldsValueRepository>();
            valueRepository.Setup(r => r.HasAnyValues(nodeKey)).Returns(false);

            var handler = new MetaFieldsValueDiskRefresherHandler(diskService.Object, factory.Object, valueRepository.Object);
            await handler.HandleAsync(new MetaFieldsValueChangedNotification(nodeKey), CancellationToken.None);

            diskService.Verify(d => d.DeleteArtifacts(
                It.Is<IEnumerable<IArtifact>>(a => a.Any(x =>
                    x.Udi.EntityType == SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue
                    && ((GuidUdi)x.Udi).Guid == nodeKey))), Times.Once);
            diskService.Verify(d => d.WriteArtifactsAsync(
                It.IsAny<IEnumerable<IArtifact>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SitemapContentChanged_WithSettings_WritesArtifact()
        {
            var nodeKey = Guid.NewGuid();
            var artifact = Mock.Of<IArtifact>();
            var connector = new Mock<IServiceConnector>();
            connector.Setup(c => c.GetArtifactAsync(
                    It.Is<Udi>(u => u.EntityType == SeoToolkitDeployConstants.UdiEntityType.SitemapContent),
                    It.IsAny<IContextCache>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(artifact);
            var factory = new Mock<IServiceConnectorFactory>();
            factory.Setup(f => f.GetConnector(SeoToolkitDeployConstants.UdiEntityType.SitemapContent))
                .Returns(connector.Object);
            var diskService = new Mock<IDiskEntityService>();
            var sitemapService = new Mock<ISitemapService>();
            sitemapService.Setup(s => s.GetContentSettings(nodeKey))
                .Returns(new SitemapContentSettings { NodeKey = nodeKey, ExcludeFromSitemap = true });

            var handler = new SitemapContentDiskRefresherHandler(diskService.Object, factory.Object, sitemapService.Object);
            await handler.HandleAsync(new SitemapContentChangedNotification(nodeKey), CancellationToken.None);

            diskService.Verify(d => d.WriteArtifactsAsync(
                It.Is<IEnumerable<IArtifact>>(a => a.Contains(artifact)), It.IsAny<CancellationToken>()), Times.Once);
            diskService.Verify(d => d.DeleteArtifacts(It.IsAny<IEnumerable<IArtifact>>()), Times.Never);
        }

        [Test]
        public async Task SitemapContentChanged_ResetToDefault_DeletesArtifact()
        {
            var nodeKey = Guid.NewGuid();
            var factory = new Mock<IServiceConnectorFactory>();
            var diskService = new Mock<IDiskEntityService>();
            var sitemapService = new Mock<ISitemapService>();
            // Reset-to-default: the service deleted the row, so GetContentSettings returns null.
            sitemapService.Setup(s => s.GetContentSettings(nodeKey)).Returns((SitemapContentSettings?)null);

            var handler = new SitemapContentDiskRefresherHandler(diskService.Object, factory.Object, sitemapService.Object);
            await handler.HandleAsync(new SitemapContentChangedNotification(nodeKey), CancellationToken.None);

            diskService.Verify(d => d.DeleteArtifacts(
                It.Is<IEnumerable<IArtifact>>(a => a.Any(x =>
                    x.Udi.EntityType == SeoToolkitDeployConstants.UdiEntityType.SitemapContent
                    && ((GuidUdi)x.Udi).Guid == nodeKey))), Times.Once);
            diskService.Verify(d => d.WriteArtifactsAsync(
                It.IsAny<IEnumerable<IArtifact>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
