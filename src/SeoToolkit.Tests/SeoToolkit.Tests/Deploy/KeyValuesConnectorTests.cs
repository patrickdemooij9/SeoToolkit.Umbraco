using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Repositories.SeoKeyValueRepository;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class KeyValuesConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings(bool pruneMissing = false)
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings { PruneMissing = pruneMissing });
            return monitor.Object;
        }

        [Test]
        public async Task RootKeyValues_UseWellKnownGuid_AndSetOnImport()
        {
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["siteName"] = "My Site" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.KeyValues, SeoToolkitDeployConstants.RootKeyValuesGuid);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.Values["siteName"], Is.EqualTo("My Site"));
            Assert.That(artifact.DomainCollectionUdi, Is.Null);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            repository.Verify(r => r.Set("siteName", "My Site", null), Times.Once);
        }

        [Test]
        public async Task Process_PruneMissing_DeletesTargetKeysAbsentFromArtifact()
        {
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["keep"] = "target", ["drop"] = "target" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, DefaultSettings(pruneMissing: true));

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
        public async Task Process_OverwriteOnly_DoesNotDeleteTargetKeys()
        {
            var repository = new Mock<ISeoKeyValueRepository>();
            repository.Setup(r => r.Get((Guid?)null))
                .Returns(new Dictionary<string, string> { ["drop"] = "target" });
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.GetAll()).Returns([]);

            var connector = new SeoToolkitKeyValuesServiceConnector(
                repository.Object, domainsService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.KeyValues, SeoToolkitDeployConstants.RootKeyValuesGuid);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.KeyValuesArtifact(udi)
            {
                Name = "SeoToolkit key/values (global)",
                Values = new Dictionary<string, string> { ["keep"] = "source" },
            };

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            repository.Verify(r => r.Delete(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
        }
    }
}
