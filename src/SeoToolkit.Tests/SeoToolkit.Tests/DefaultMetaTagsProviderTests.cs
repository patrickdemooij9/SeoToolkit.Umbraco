using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldSuggestions;
using SeoToolkit.Umbraco.MetaFields.Core.Providers;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class DefaultMetaTagsProviderTests
    {
        private static readonly string LongDescription =
            "This course teaches the essentials of bookkeeping for small businesses across Australia. " +
            "You will learn how to reconcile accounts, prepare BAS statements and run payroll with confidence.";

        private readonly Guid _contentKey = Guid.NewGuid();
        private readonly Guid _contentTypeKey = Guid.NewGuid();
        private Mock<IMetaFieldsValueService> _valueService = null!;
        private SeoDescriptionField _descriptionField = null!;

        [SetUp]
        public void SetUp()
        {
            _valueService = new Mock<IMetaFieldsValueService>();
            _descriptionField = new SeoDescriptionField();
        }

        [Test]
        public void Get_LeavesFallbackDescriptionUncutByDefault()
        {
            var metaTags = CreateProvider().Get(CreateContent(), true);

            Assert.That(metaTags.GetValue<string>(SeoFieldAliasConstants.MetaDescription), Is.EqualTo(LongDescription));
        }

        [Test]
        public void Get_TruncatesFallbackDescription_WhenEnabled()
        {
            EnableTruncateFallbackValue();

            var metaTags = CreateProvider().Get(CreateContent(), true);

            Assert.That(metaTags.GetValue<string>(SeoFieldAliasConstants.MetaDescription),
                Is.EqualTo("This course teaches the essentials of bookkeeping for small businesses across Australia."));
        }

        [Test]
        public void Get_LeavesEditorDescriptionUncut_WhenEnabled()
        {
            EnableTruncateFallbackValue();
            _valueService.Setup(s => s.GetUserValues(_contentKey, It.IsAny<string>()))
                .Returns(new Dictionary<string, object> { { SeoFieldAliasConstants.MetaDescription, LongDescription } });

            var metaTags = CreateProvider().Get(CreateContent(), true);

            Assert.That(metaTags.GetValue<string>(SeoFieldAliasConstants.MetaDescription), Is.EqualTo(LongDescription));
        }

        private void EnableTruncateFallbackValue()
        {
            _descriptionField.Suggestions.OfType<SeoFieldMaxLengthSuggestion>().First().TruncateFallbackValue = true;
        }

        private DefaultMetaTagsProvider CreateProvider()
        {
            var contentType = new Mock<IContentType>();
            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(s => s.Get(_contentTypeKey)).Returns(contentType.Object);

            var seoSettingsService = new Mock<ISeoSettingsService>();
            seoSettingsService.Setup(s => s.IsEnabled(contentType.Object)).Returns(true);

            var settings = new DocumentTypeSettingsDto();
            settings.Fields[_descriptionField] = new DocumentTypeValueDto { Value = LongDescription };
            var settingsService = new Mock<IMetaFieldsSettingsService>();
            settingsService.Setup(s => s.Get(_contentTypeKey)).Returns(settings);

            var converters = new SeoConverterCollection(() => new ISeoValueConverter[]
            {
                new TextSeoValueConverter(Mock.Of<ISeoDomainResolver>())
            });

            return new DefaultMetaTagsProvider(
                settingsService.Object,
                new SeoFieldCollection(() => new ISeoField[] { _descriptionField }),
                _valueService.Object,
                converters,
                NullLogger<DefaultMetaTagsProvider>.Instance,
                Mock.Of<IProfiler>(),
                seoSettingsService.Object,
                contentTypeService.Object,
                Mock.Of<IEventAggregator>());
        }

        private IPublishedContent CreateContent()
        {
            var contentType = new Mock<IPublishedContentType>();
            contentType.Setup(t => t.Key).Returns(_contentTypeKey);

            var content = new Mock<IPublishedContent>();
            content.Setup(c => c.Key).Returns(_contentKey);
            content.Setup(c => c.ContentType).Returns(contentType.Object);
            return content.Object;
        }
    }
}
