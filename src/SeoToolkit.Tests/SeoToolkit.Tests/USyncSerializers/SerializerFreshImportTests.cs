using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using SeoToolkit.Umbraco.uSync;
using SeoToolkit.Umbraco.uSync.Serializers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using uSync.Core.Serialization;

namespace SeoToolkit.Tests.USyncSerializers
{
    /// <summary>
    /// Exports an item from a "source" environment and imports it into a "target" environment that has the
    /// same content type but no stored SeoToolkit settings yet (issue #581).
    /// </summary>
    [TestFixture]
    public class SerializerFreshImportTests
    {
        private const string ContentTypeAlias = "homePage";
        private static readonly Guid ContentTypeKey = Guid.NewGuid();

        private static Mock<IContentTypeService> ContentTypeService()
        {
            var contentType = new Mock<IContentType>();
            contentType.SetupGet(c => c.Key).Returns(ContentTypeKey);
            contentType.SetupGet(c => c.Alias).Returns(ContentTypeAlias);
            contentType.SetupGet(c => c.Name).Returns("Home page");

            var service = new Mock<IContentTypeService>();
            service.Setup(s => s.Get(ContentTypeKey)).Returns(contentType.Object);
            service.Setup(s => s.Get(ContentTypeAlias)).Returns(contentType.Object);
            service.Setup(s => s.GetAsync(ContentTypeKey)).ReturnsAsync(contentType.Object);
            return service;
        }

        [Test]
        public async Task SitemapSettings_ImportsOnTargetWithoutExistingSettings()
        {
            var contentTypeService = ContentTypeService();
            var source = new SitemapSettingsSerializer(NullLogger<SyncSerializerRoot<SitemapPageSettings>>.Instance,
                Mock.Of<ISitemapService>(), contentTypeService.Object);
            var exported = await source.SerializeAsync(new SitemapPageSettings
            {
                ContentTypeGuid = ContentTypeKey,
                HideFromSitemap = true,
                ChangeFrequency = "daily",
                Priority = 0.5
            }, new SyncSerializerOptions());

            var target = new SitemapSettingsSerializer(NullLogger<SyncSerializerRoot<SitemapPageSettings>>.Instance,
                Mock.Of<ISitemapService>(), contentTypeService.Object);
            var result = await target.DeserializeAsync(exported.Item!, new SyncSerializerOptions());

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Item!.ContentTypeGuid, Is.EqualTo(ContentTypeKey));
            Assert.That(result.Item.HideFromSitemap, Is.True);
            Assert.That(result.Item.ChangeFrequency, Is.EqualTo("daily"));
            Assert.That(result.Item.Priority, Is.EqualTo(0.5));
        }

        [Test]
        public async Task MetaFields_ImportsOnTargetWithoutExistingSettings()
        {
            var contentTypeService = ContentTypeService();
            var fields = new SeoFieldCollection(() => []);
            var source = new MetaFieldsSerializer(NullLogger<SyncSerializerRoot<DocumentTypeSettingsDto>>.Instance,
                Mock.Of<IMetaFieldsSettingsService>(), contentTypeService.Object, fields);
            var exported = await source.SerializeAsync(new DocumentTypeSettingsDto
            {
                Content = contentTypeService.Object.Get(ContentTypeKey)!
            }, new SyncSerializerOptions());

            var target = new MetaFieldsSerializer(NullLogger<SyncSerializerRoot<DocumentTypeSettingsDto>>.Instance,
                Mock.Of<IMetaFieldsSettingsService>(), contentTypeService.Object, fields);
            var result = await target.DeserializeAsync(exported.Item!, new SyncSerializerOptions());

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Item!.Content.Key, Is.EqualTo(ContentTypeKey));
        }

        [Test]
        public async Task SeoSetting_ImportsWhenContentTypeLookupReturnsNothing()
        {
            // FindItemAsync returns null here, forcing the fallback path that reads the key from the node.
            var contentTypeService = ContentTypeService();
            var source = new SeoSettingSerializer(NullLogger<SyncSerializerRoot<SeoSetting>>.Instance,
                Mock.Of<ISeoSettingsService>(), contentTypeService.Object);
            var exported = await source.SerializeAsync(new SeoSetting
            {
                ContentTypeKey = ContentTypeKey,
                IsEnabled = true
            }, new SyncSerializerOptions());

            var target = new SeoSettingSerializer(NullLogger<SyncSerializerRoot<SeoSetting>>.Instance,
                Mock.Of<ISeoSettingsService>(), Mock.Of<IContentTypeService>());
            var result = await target.DeserializeAsync(exported.Item!, new SyncSerializerOptions());

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(result.Item!.ContentTypeKey, Is.EqualTo(ContentTypeKey));
            Assert.That(result.Item.IsEnabled, Is.True);
        }
    }
}
