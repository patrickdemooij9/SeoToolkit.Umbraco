using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Artifacts;
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
    public class SitemapPageTypeConnectorTests
    {
        private Mock<ISitemapService> _sitemapService = null!;
        private Mock<IContentTypeService> _contentTypeService = null!;
        private SeoToolkitSitemapPageTypeServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _sitemapService = new Mock<ISitemapService>();
            _contentTypeService = new Mock<IContentTypeService>();
            _connector = new SeoToolkitSitemapPageTypeServiceConnector(
                _sitemapService.Object, _contentTypeService.Object, DefaultSettings());
        }

        [Test]
        public async Task RoundTrip_PreservesAllSitemapPageTypeFields()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("News Page");
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            _sitemapService.Setup(s => s.GetPageTypeSettings(contentTypeKey)).Returns(new SitemapPageSettings
            {
                ContentTypeGuid = contentTypeKey,
                HideFromSitemap = true,
                ChangeFrequency = "weekly",
                Priority = 0.4,
            });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, contentTypeKey);
            var artifact = await _connector.GetArtifactAsync(udi, PassThroughCache.Instance);
            Assert.That(artifact, Is.Not.Null);

            SitemapPageSettings? saved = null;
            _sitemapService.Setup(s => s.SetPageTypeSettings(It.IsAny<SitemapPageSettings>()))
                .Callback<SitemapPageSettings>(s => saved = s);

            var state = await _connector.ProcessInitAsync(artifact!, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            Assert.That(saved, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(saved!.ContentTypeGuid, Is.EqualTo(contentTypeKey));
                Assert.That(saved.HideFromSitemap, Is.True);
                Assert.That(saved.ChangeFrequency, Is.EqualTo("weekly"));
                Assert.That(saved.Priority, Is.EqualTo(0.4));
            });
        }
    }
}
