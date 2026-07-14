using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using SeoToolkit.Umbraco.ScriptManager.Core.Notifications;
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
    }
}
