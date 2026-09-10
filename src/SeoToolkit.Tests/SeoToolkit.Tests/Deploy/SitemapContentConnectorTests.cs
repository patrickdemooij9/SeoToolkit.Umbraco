using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SitemapContentConnectorTests
    {
        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [Test]
        public async Task RoundTrip_PreservesOverridesAndDependsOnDocument()
        {
            var nodeKey = Guid.NewGuid();
            var content = new Mock<IContent>();
            content.SetupGet(c => c.Key).Returns(nodeKey);
            content.SetupGet(c => c.Name).Returns("News item");
            var contentService = new Mock<IContentService>();
            contentService.Setup(s => s.GetById(nodeKey)).Returns(content.Object);

            var sitemapService = new Mock<ISitemapService>();
            sitemapService.Setup(s => s.GetContentSettings(nodeKey)).Returns(new SitemapContentSettings
            {
                NodeKey = nodeKey,
                ExcludeFromSitemap = true,
                ChangeFrequency = "daily",
                Priority = 0.9,
            });

            var connector = new SeoToolkitSitemapContentServiceConnector(
                sitemapService.Object, contentService.Object, DefaultSettings());

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, nodeKey);
            var artifact = await connector.GetArtifactAsync(udi, PassThroughCache.Instance);

            Assert.That(artifact, Is.Not.Null);
            Assert.That(artifact!.Dependencies.Select(d => d.Udi),
                Does.Contain(new GuidUdi(Constants.UdiEntityType.Document, nodeKey)));

            SitemapContentSettings? saved = null;
            sitemapService.Setup(s => s.SetContentSettings(It.IsAny<SitemapContentSettings>()))
                .Callback<SitemapContentSettings>(s => saved = s);

            var state = await connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 7);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.NodeKey, Is.EqualTo(nodeKey));
                Assert.That(saved.ExcludeFromSitemap, Is.True);
                Assert.That(saved.ChangeFrequency, Is.EqualTo("daily"));
                Assert.That(saved.Priority, Is.EqualTo(0.9));
            });
        }
    }
}
