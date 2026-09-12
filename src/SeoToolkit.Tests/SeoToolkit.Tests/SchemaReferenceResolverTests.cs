using Moq;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SchemaReferenceResolverTests
    {
        [Test]
        public void ResolveReference_ResolvesPageName()
        {
            var content = CreateContent("Home");

            Assert.That(SchemaReferenceResolver.ResolveReference("[PageName]", content), Is.EqualTo("Home"));
        }

        [Test]
        public void ResolveReference_ResolvesContentProperty()
        {
            var content = CreateContent("Home", ("title", "Welcome home"));

            Assert.That(SchemaReferenceResolver.ResolveReference("[Property:title]", content), Is.EqualTo("Welcome home"));
        }

        [Test]
        public void ResolveReference_ReturnsEmptyForAPropertyTheContentDoesNotHave()
        {
            var content = CreateContent("Home");

            Assert.That(SchemaReferenceResolver.ResolveReference("[Property:title]", content), Is.Empty);
        }

        [Test]
        public void ResolveReference_ReturnsEmptyWithoutContent()
        {
            Assert.That(SchemaReferenceResolver.ResolveReference("[PageName]", null), Is.Empty);
        }

        [Test]
        public void ResolveTokens_CombinesTextWithAContextToken()
        {
            var content = CreateContent("Home");

            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {pageName}", content), Is.EqualTo("Hello Home"));
        }

        [Test]
        public void ResolveTokens_ResolvesAContentPropertyByItsAlias()
        {
            var content = CreateContent("Home", ("title", "Welcome home"));

            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {title}!", content), Is.EqualTo("Hello Welcome home!"));
            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {property:title}!", content), Is.EqualTo("Hello Welcome home!"));
        }

        [Test]
        public void ResolveTokens_ResolvesEveryTokenInTheValue()
        {
            var content = CreateContent("Home", ("title", "Welcome home"));

            Assert.That(SchemaReferenceResolver.ResolveTokens("{pageName} - {title}", content), Is.EqualTo("Home - Welcome home"));
        }

        [Test]
        public void ResolveTokens_PrefersAContextTokenOverAPropertyOfTheSameName()
        {
            var content = CreateContent("Home", ("pageName", "Some property value"));

            Assert.That(SchemaReferenceResolver.ResolveTokens("{pageName}", content), Is.EqualTo("Home"));
        }

        [Test]
        public void ResolveTokens_LeavesUnknownTokensUntouched()
        {
            var content = CreateContent("Home");

            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {notAToken}", content), Is.EqualTo("Hello {notAToken}"));
        }

        [Test]
        public void ResolveTokens_LeavesTextThatOnlyLooksLikeATokenUntouched()
        {
            var content = CreateContent("Home");

            const string json = "{\"@type\": \"Thing\", \"name\": \"Test\"}";
            Assert.That(SchemaReferenceResolver.ResolveTokens(json, content), Is.EqualTo(json));
        }

        [Test]
        public void ResolveTokens_ResolvesAnEmptyPropertyToNothing()
        {
            var content = CreateContent("Home", ("title", null));

            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {title}", content), Is.EqualTo("Hello "));
        }

        [Test]
        public void ResolveTokens_ReturnsTheValueUnchangedWithoutContent()
        {
            Assert.That(SchemaReferenceResolver.ResolveTokens("Hello {pageName}", null), Is.EqualTo("Hello {pageName}"));
        }

        private static IPublishedContent CreateContent(string name, params (string Alias, string? Value)[] properties)
        {
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.Name).Returns(name);

            foreach (var (alias, value) in properties)
            {
                var property = new Mock<IPublishedProperty>();
                property.Setup(x => x.GetValue(null, null)).Returns(value);
                content.Setup(x => x.GetProperty(alias)).Returns(property.Object);
            }

            return content.Object;
        }
    }
}
