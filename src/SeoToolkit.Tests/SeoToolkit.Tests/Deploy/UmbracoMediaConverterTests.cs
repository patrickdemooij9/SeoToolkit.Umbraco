using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class UmbracoMediaConverterTests
    {
        private static UmbracoMediaConverter CreateConverter()
            => new(Mock.Of<IUmbracoContextFactory>());

        private static IPublishedContent Media(Guid key)
        {
            var media = new Mock<IPublishedContent>();
            media.SetupGet(m => m.Key).Returns(key);
            return media.Object;
        }

        [Test]
        public void ConvertObjectToEditorValue_IsDeterministic_ForStableChecksum()
        {
            var converter = CreateConverter();
            var media = Media(Guid.NewGuid());

            // Two exports of the same value must serialize identically, or the Deploy artifact
            // checksum would change on every export and re-transfer for no reason.
            var first = JsonConvert.SerializeObject(converter.ConvertObjectToEditorValue(media));
            var second = JsonConvert.SerializeObject(converter.ConvertObjectToEditorValue(media));

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void GetReferencedMediaKeys_ReadsKey_FromObjectAndDatabaseForms()
        {
            var converter = CreateConverter();
            var key = Guid.NewGuid();

            Assert.Multiple(() =>
            {
                // Object form (what the settings connector holds).
                Assert.That(converter.GetReferencedMediaKeys(Media(key)), Is.EqualTo(new[] { key }));
                // Database form (bare GUID, what the values connector holds).
                Assert.That(converter.GetReferencedMediaKeys(key.ToString()), Is.EqualTo(new[] { key }));
            });
        }

        [Test]
        public void GetReferencedMediaKeys_ReturnsNothing_ForNonMediaValue()
        {
            var converter = CreateConverter();
            Assert.That(converter.GetReferencedMediaKeys("not-a-guid"), Is.Empty);
        }

        [Test]
        public void ImplementsMediaReferenceConverter()
        {
            Assert.That(CreateConverter(), Is.InstanceOf<IMediaReferenceConverter>());
        }
    }
}
