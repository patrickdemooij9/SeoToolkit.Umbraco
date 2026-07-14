using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
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
    public class SeoSettingConnectorTests
    {
        private Mock<ISeoSettingsService> _seoSettingsService = null!;
        private Mock<IContentTypeService> _contentTypeService = null!;
        private SeoToolkitSeoSettingServiceConnector _connector = null!;

        private static IOptionsMonitor<SeoToolkitDeploySettings> DefaultSettings()
        {
            var monitor = new Mock<IOptionsMonitor<SeoToolkitDeploySettings>>();
            monitor.Setup(m => m.CurrentValue).Returns(new SeoToolkitDeploySettings());
            return monitor.Object;
        }

        [SetUp]
        public void SetUp()
        {
            _seoSettingsService = new Mock<ISeoSettingsService>();
            _seoSettingsService.Setup(s => s.GetAll()).Returns(new Dictionary<Guid, bool>());
            _contentTypeService = new Mock<IContentTypeService>();
            _connector = new SeoToolkitSeoSettingServiceConnector(
                _seoSettingsService.Object, _contentTypeService.Object, DefaultSettings());
        }

        [Test]
        public async Task GetArtifact_BuildsEnabledFlagAndDocTypeDependency()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            contentType.SetupGet(c => c.Name).Returns("Home Page");
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);
            _seoSettingsService.Setup(s => s.GetAll()).Returns(new Dictionary<Guid, bool> { [contentTypeKey] = true });

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = await _connector.GetArtifactAsync(udi, Mock.Of<IContextCache>());

            Assert.That(artifact, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(artifact!.Enabled, Is.True);
                Assert.That(artifact.Udi, Is.EqualTo(udi));
                Assert.That(artifact.Dependencies.Select(d => d.Udi),
                    Does.Contain(new GuidUdi(Constants.UdiEntityType.DocumentType, contentTypeKey)));
            });
        }

        [Test]
        public async Task Process_Pass2_TogglesSeoSettings()
        {
            var contentTypeKey = Guid.NewGuid();
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(contentTypeKey);
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns(contentType.Object);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.SeoSettingArtifact(udi) { Enabled = true, Name = "Home Page" };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            await _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2);

            _seoSettingsService.Verify(s => s.ToggleSeoSettings(contentTypeKey, true), Times.Once);
        }

        [Test]
        public async Task Process_Pass2_MissingContentType_SkipsWithoutThrowing()
        {
            var contentTypeKey = Guid.NewGuid();
            _contentTypeService.Setup(s => s.Get(contentTypeKey)).Returns((IContentType?)null);

            var udi = new GuidUdi(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, contentTypeKey);
            var artifact = new SeoToolkit.Umbraco.Deploy.Artifacts.SeoSettingArtifact(udi) { Enabled = true, Name = "Gone" };

            var state = await _connector.ProcessInitAsync(artifact, Mock.Of<IDeployContext>());
            Assert.DoesNotThrowAsync(() => _connector.ProcessAsync(state, Mock.Of<IDeployContext>(), 2));
            _seoSettingsService.Verify(s => s.ToggleSeoSettings(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
