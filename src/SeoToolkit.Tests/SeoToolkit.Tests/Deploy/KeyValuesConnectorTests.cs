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
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
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
    }
}
