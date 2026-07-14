using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class DomainCollectionConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        private static IDomain MakeDomain(int id, string name)
        {
            var domain = new Mock<IDomain>();
            domain.SetupGet(d => d.Id).Returns(id);
            domain.SetupGet(d => d.DomainName).Returns(name);
            return domain.Object;
        }

        [Test]
        public async Task RoundTrip_MapsDomainIdsToNamesAndBack()
        {
            var collectionId = Guid.NewGuid();
            var domainsService = new Mock<ISeoDomainsService>();
            domainsService.Setup(s => s.Get(collectionId)).Returns(new SeoDomainCollection
            {
                Id = collectionId,
                Name = "Main site",
                DomainIds = [11, 22],
                Settings = new Dictionary<string, string> { ["someSetting"] = "true" },
            });

            var domainService = new Mock<IDomainService>();
            domainService.Setup(s => s.GetAllAsync(true)).ReturnsAsync(
            [
                MakeDomain(11, "example.com"),
                MakeDomain(22, "example.co.uk"),
                MakeDomain(33, "other.com"),
            ]);

            var connector = new SeoToolkitDomainCollectionServiceConnector(
                domainsService.Object, domainService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, collectionId);
            var artifact = await connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.DomainNames, Is.EquivalentTo(new[] { "example.com", "example.co.uk" }));

            SeoDomainCollection? saved = null;
            domainsService.Setup(s => s.Save(It.IsAny<SeoDomainCollection>()))
                .Callback<SeoDomainCollection>(c => saved = c).Returns(collectionId);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.Id, Is.EqualTo(collectionId));
                Assert.That(saved.Name, Is.EqualTo("Main site"));
                Assert.That(saved.DomainIds, Is.EquivalentTo(new[] { 11, 22 }));
                Assert.That(saved.Settings["someSetting"], Is.EqualTo("true"));
            });
        }
    }
}
