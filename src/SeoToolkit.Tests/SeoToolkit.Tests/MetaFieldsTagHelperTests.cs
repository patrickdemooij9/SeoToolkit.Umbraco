using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using SeoToolkit.Umbraco.MetaFields.Core.Common.TagHelpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class MetaFieldsTagHelperTests
    {
        private Mock<IMetaFieldsService> _metaFieldsService = null!;

        [SetUp]
        public void SetUp()
        {
            _metaFieldsService = new Mock<IMetaFieldsService>();
        }

        [Test]
        public void Process_WithNullPublishedRequest_DoesNotThrow()
        {
            // Arrange - request not routed to published content (e.g. a custom route), so PublishedRequest is null.
            var helper = new MetaFieldsTagHelper(_metaFieldsService.Object, GetContextFactory(publishedRequest: null));
            var output = CreateOutput();

            // Act / Assert - must not throw (regression for #559) and must strip the tag without calling the service.
            Assert.DoesNotThrow(() => helper.Process(CreateContext(), output));
            Assert.That(output.TagName, Is.Null);
            _metaFieldsService.Verify(s => s.Get(It.IsAny<IPublishedContent>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void Process_WithNullPublishedContent_DoesNotThrow()
        {
            // Arrange - request routed but without published content.
            var publishedRequest = new Mock<IPublishedRequest>();
            publishedRequest.Setup(r => r.PublishedContent).Returns((IPublishedContent)null!);

            var helper = new MetaFieldsTagHelper(_metaFieldsService.Object, GetContextFactory(publishedRequest.Object));
            var output = CreateOutput();

            // Act / Assert
            Assert.DoesNotThrow(() => helper.Process(CreateContext(), output));
            Assert.That(output.TagName, Is.Null);
            _metaFieldsService.Verify(s => s.Get(It.IsAny<IPublishedContent>(), It.IsAny<bool>()), Times.Never);
        }

        private static IUmbracoContextFactory GetContextFactory(IPublishedRequest? publishedRequest)
        {
            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.Setup(c => c.PublishedRequest).Returns(publishedRequest);

            var factory = new Mock<IUmbracoContextFactory>();
            factory.Setup(f => f.EnsureUmbracoContext())
                .Returns(() => new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>()));

            return factory.Object;
        }

        private static TagHelperContext CreateContext()
        {
            return new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString());
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "meta-fields",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }
    }
}
