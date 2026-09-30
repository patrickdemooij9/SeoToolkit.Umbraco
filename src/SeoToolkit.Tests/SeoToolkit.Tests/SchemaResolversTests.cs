using Moq;
using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SchemaResolversTests
    {
        private readonly UmbracoMediaConverter _mediaConverter = new(Mock.Of<IUmbracoContextFactory>());

        [Test]
        public void WebSite_RendersTheSiteNameProperties()
        {
            var json = new WebSiteSchemaResolver().ToSchema(new Dictionary<string, object>
            {
                ["name"] = "Example",
                ["alternateName"] = "EX",
                ["url"] = "https://example.com/",
                ["publisher"] = new IThing[] { new Organization { Name = "Example Inc" } }
            }).ToString();

            Assert.That(json, Is.EqualTo("{\"@context\":\"https://schema.org\",\"@type\":\"WebSite\",\"name\":\"Example\",\"alternateName\":\"EX\",\"url\":\"https://example.com/\",\"publisher\":{\"@type\":\"Organization\",\"name\":\"Example Inc\"}}"));
        }

        [Test]
        public void WebSite_LeavesOutEmptyValues()
        {
            var json = new WebSiteSchemaResolver().ToSchema(new Dictionary<string, object>
            {
                ["name"] = "Example",
                ["url"] = "",
                ["description"] = null!
            }).ToString();

            Assert.That(json, Is.EqualTo("{\"@context\":\"https://schema.org\",\"@type\":\"WebSite\",\"name\":\"Example\"}"));
        }

        [Test]
        public void WebPage_RendersDatesAndIsPartOf()
        {
            var json = new WebPageSchemaResolver(_mediaConverter).ToSchema(new Dictionary<string, object>
            {
                ["name"] = "About",
                ["datePublished"] = "2024-03-01T09:30:00+01:00",
                ["isPartOf"] = new IThing[] { new WebSite { Name = "Example" } }
            }).ToString();

            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("\"@type\":\"WebPage\""));
                Assert.That(json, Does.Contain("\"datePublished\":\"2024-03-01T09:30:00+01:00\""));
                Assert.That(json, Does.Contain("\"isPartOf\":{\"@type\":\"WebSite\",\"name\":\"Example\"}"));
                Assert.That(json, Does.Not.Contain("dateModified"));
            });
        }

        [TestCase("article", "Article")]
        [TestCase("blogPosting", "BlogPosting")]
        [TestCase("newsArticle", "NewsArticle")]
        public void Articles_RenderTheGoogleArticleProperties(string alias, string type)
        {
            ISchemaResolver resolver = alias switch
            {
                "article" => new ArticleSchemaResolver(_mediaConverter),
                "blogPosting" => new BlogPostingSchemaResolver(_mediaConverter),
                _ => new NewsArticleSchemaResolver(_mediaConverter)
            };

            var json = resolver.ToSchema(new Dictionary<string, object>
            {
                ["headline"] = "Hello world",
                ["image"] = "https://example.com/media/hello.jpg",
                ["datePublished"] = "2024-03-01T09:30:00+00:00",
                ["author"] = new IThing[] { new ExtendedPerson { Name = "Jane", JobTitle = "Editor" } }
            }).ToString();

            Assert.Multiple(() =>
            {
                Assert.That(resolver.Alias, Is.EqualTo(alias));
                Assert.That(json, Does.Contain($"\"@type\":\"{type}\""));
                Assert.That(json, Does.Contain("\"headline\":\"Hello world\""));
                Assert.That(json, Does.Contain("\"image\":\"https://example.com/media/hello.jpg\""));
                Assert.That(json, Does.Contain("\"datePublished\":\"2024-03-01T09:30:00+00:00\""));
                Assert.That(json, Does.Contain("\"author\":{\"@type\":\"Person\",\"name\":\"Jane\",\"jobTitle\":\"Editor\"}"));
                Assert.That(json, Does.Not.Contain("publisher"));
            });
        }

        [Test]
        public void Person_RendersSameAsLinksPerLine()
        {
            var json = new PersonSchemaResolver(_mediaConverter).ToSchema(new Dictionary<string, object>
            {
                ["name"] = "Jane",
                ["jobTitle"] = "Editor",
                ["sameAs"] = "https://www.linkedin.com/in/jane\r\n\r\nnot a url\nhttps://github.com/jane",
                ["worksFor"] = new IThing[] { new Organization { Name = "Example Inc" } }
            }).ToString();

            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("\"@type\":\"Person\""));
                Assert.That(json, Does.Contain("\"jobTitle\":\"Editor\""));
                Assert.That(json, Does.Contain("\"sameAs\":[\"https://www.linkedin.com/in/jane\",\"https://github.com/jane\"]"));
                Assert.That(json, Does.Contain("\"worksFor\":{\"@type\":\"Organization\",\"name\":\"Example Inc\"}"));
            });
        }

        [Test]
        public void ImageObject_RendersTheImageLicenseProperties()
        {
            var json = new ImageObjectSchemaResolver(_mediaConverter).ToSchema(new Dictionary<string, object>
            {
                ["contentUrl"] = "https://example.com/media/photo.jpg",
                ["creditText"] = "Jane Doe",
                ["copyrightNotice"] = "© Example Inc",
                ["license"] = "https://example.com/license",
                ["acquireLicensePage"] = "https://example.com/buy"
            }).ToString();

            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("\"@type\":\"ImageObject\""));
                Assert.That(json, Does.Contain("\"contentUrl\":\"https://example.com/media/photo.jpg\""));
                Assert.That(json, Does.Contain("\"license\":\"https://example.com/license\""));
                Assert.That(json, Does.Contain("\"creditText\":\"Jane Doe\""));
                Assert.That(json, Does.Contain("\"acquireLicensePage\":\"https://example.com/buy\""));
            });
        }

        [Test]
        public void ImageObject_IsNotRenderedWithoutAnImage()
        {
            var schema = new ImageObjectSchemaResolver(_mediaConverter).ToSchema(new Dictionary<string, object>
            {
                ["name"] = "Photo"
            });

            Assert.That(schema, Is.Null);
        }

        [Test]
        public void LocalBusiness_RendersAddressGeoAndOpeningHours()
        {
            var openingHours = new OpeningHoursSpecificationSchemaResolver().ToSchema(new Dictionary<string, object>
            {
                ["dayOfWeek"] = new[] { "Monday", "friday", "NotADay" },
                ["opens"] = "09:00",
                ["closes"] = "17:30"
            });

            var json = new LocalBusinessSchemaResolver(_mediaConverter).ToSchema(new Dictionary<string, object>
            {
                ["name"] = "Bakery",
                ["telephone"] = "+31 20 123 4567",
                ["latitude"] = "52.3676",
                ["longitude"] = "4.9041",
                ["address"] = new IThing[] { new PostalAddress { StreetAddress = "Main street 1" } },
                ["openingHoursSpecification"] = new[] { openingHours }
            }).ToString();

            Assert.Multiple(() =>
            {
                Assert.That(json, Does.Contain("\"@type\":\"LocalBusiness\""));
                Assert.That(json, Does.Contain("\"address\":{\"@type\":\"PostalAddress\",\"streetAddress\":\"Main street 1\"}"));
                Assert.That(json, Does.Contain("\"geo\":{\"@type\":\"GeoCoordinates\",\"latitude\":52.3676,\"longitude\":4.9041}"));
                Assert.That(json, Does.Contain("\"dayOfWeek\":[\"https://schema.org/Monday\",\"https://schema.org/Friday\"]"));
                Assert.That(json, Does.Contain("\"opens\":\"09:00:00\""));
                Assert.That(json, Does.Contain("\"closes\":\"17:30:00\""));
                Assert.That(json, Does.Not.Contain("\"image\""));
            });
        }
    }
}
