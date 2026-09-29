using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SeoKeyValueService;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Events;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class KeyValuesConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        private static SeoKeyValueService CreateService(ISeoKeyValueRepository repository, IEventAggregator eventAggregator)
            => new(Mock.Of<ISeoDomainResolver>(), repository, eventAggregator);

        [Test]
        public async Task RootKeyValues_UseWellKnownGuid_AndSetOnImport()
        {
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["siteName"] = "My Site" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);
            var eventAggregator = new Mock<IEventAggregator>();

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, CreateService(repository.Object, eventAggregator.Object), DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.KeyValues, SeoToolkitDeployConstants.RootKeyValuesGuid);
            var artifact = await connector.GetArtifactAsync(udi, PassThroughCache.Instance);

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.Values["siteName"], Is.EqualTo("My Site"));
            Assert.That(artifact.DomainCollectionUdi, Is.Null);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            repository.Verify(r => r.Set("siteName", "My Site", null), Times.Once);
            // Saving goes through the service, which publishes the notification once to keep the
            // target's key/values .uda in sync.
            eventAggregator.Verify(e => e.Publish(
                It.Is<SeoKeyValueSavedNotification>(n => n.DomainCollectionId == null)), Times.Once);
        }

        [Test]
        public async Task Process_ReconcilesTargetToSource_DeletesTargetKeysAbsentFromArtifact()
        {
            // Like a core schema artifact, import replaces the target to match the source exactly.
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["keep"] = "target", ["drop"] = "target" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, CreateService(repository.Object, Mock.Of<IEventAggregator>()), DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.KeyValues, SeoToolkitDeployConstants.RootKeyValuesGuid);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.KeyValuesArtifact(udi)
            {
                Name = "SeoToolkit key/values (global)",
                Values = new Dictionary<string, string> { ["keep"] = "source" },
            };

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            repository.Verify(r => r.Delete("drop", null), Times.Once);
            repository.Verify(r => r.Delete("keep", null), Times.Never);
            repository.Verify(r => r.Set("keep", "source", null), Times.Once);
        }

        [Test]
        public void SaveValues_SetsAndDeletes_AndPublishesOnce()
        {
            var domainId = Guid.NewGuid();
            var repository = new Mock<ISeoKeyValueRepository>();
            var eventAggregator = new Mock<IEventAggregator>();
            var service = CreateService(repository.Object, eventAggregator.Object);

            service.SaveValues(new Dictionary<string, string?> { ["a"] = "1", ["b"] = "", ["c"] = null, ["d"] = "4" }, domainId);

            repository.Verify(r => r.Set("a", "1", domainId), Times.Once);
            repository.Verify(r => r.Set("d", "4", domainId), Times.Once);
            repository.Verify(r => r.Delete("b", domainId), Times.Once);
            repository.Verify(r => r.Delete("c", domainId), Times.Once);
            eventAggregator.Verify(e => e.Publish(
                It.Is<SeoKeyValueSavedNotification>(n => n.DomainCollectionId == domainId)), Times.Once);
        }
    }
}
