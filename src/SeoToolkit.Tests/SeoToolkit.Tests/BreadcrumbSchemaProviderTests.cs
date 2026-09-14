using Moq;
using Schema.NET;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Providers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class BreadcrumbSchemaProviderTests
    {
        [Test]
        public void Get_BuildsTheBreadcrumbFromTheRootToTheCurrentPage()
        {
            var home = CreateContent("Home", "https://example.com/");
            var blog = CreateContent("Blog", "https://example.com/blog/");
            var post = CreateContent("My post", "https://example.com/blog/my-post/");

            var provider = CreateProvider(enabled: true, post, blog, home);

            var json = provider.Get(post)!.ToString();

            Assert.That(json, Is.EqualTo(
                "{\"@context\":\"https://schema.org\",\"@type\":\"BreadcrumbList\",\"itemListElement\":[" +
                "{\"@type\":\"ListItem\",\"name\":\"Home\",\"item\":{\"@type\":\"WebPage\",\"@id\":\"https://example.com/\"},\"position\":1}," +
                "{\"@type\":\"ListItem\",\"name\":\"Blog\",\"item\":{\"@type\":\"WebPage\",\"@id\":\"https://example.com/blog/\"},\"position\":2}," +
                "{\"@type\":\"ListItem\",\"name\":\"My post\",\"item\":{\"@type\":\"WebPage\",\"@id\":\"https://example.com/blog/my-post/\"},\"position\":3}]}"));
        }

        [Test]
        public void Get_ReturnsNullWhenDisabled()
        {
            var home = CreateContent("Home", "https://example.com/");
            var blog = CreateContent("Blog", "https://example.com/blog/");

            var provider = CreateProvider(enabled: false, blog, home);

            Assert.That(provider.Get(blog), Is.Null);
        }

        [Test]
        public void Get_ReturnsNullForASinglePage()
        {
            var home = CreateContent("Home", "https://example.com/");

            var provider = CreateProvider(enabled: true, home);

            Assert.That(provider.Get(home), Is.Null);
        }

        [Test]
        public void Get_SkipsAncestorsThatCannotBeVisited()
        {
            var home = CreateContent("Home", "https://example.com/");
            var folder = CreateContent("Folder", "https://example.com/folder/", templateId: null);
            var noUrl = CreateContent("No url", "#");
            var page = CreateContent("Page", "https://example.com/folder/no-url/page/");

            var provider = CreateProvider(enabled: true, page, noUrl, folder, home);

            var breadcrumb = (BreadcrumbList)provider.Get(page)!;
            var names = breadcrumb.ItemListElement.Value1.Select(it => ((ListItem)it).Name.First()).ToArray();
            var positions = breadcrumb.ItemListElement.Value1.Select(it => ((ListItem)it).Position.Value1.First()).ToArray();

            Assert.That(names, Is.EqualTo(new[] { "Home", "Page" }));
            Assert.That(positions, Is.EqualTo(new int?[] { 1, 2 }));
        }

        [Test]
        public void Get_KeepsAncestorsWithoutTemplateWhenTheCurrentPageHasNoTemplate()
        {
            var root = CreateContent("Root", "https://example.com/", templateId: null);
            var page = CreateContent("Page", "https://example.com/page/", templateId: null);

            var provider = CreateProvider(enabled: true, page, root);

            Assert.That(provider.Get(page), Is.Not.Null);
        }

        private static TestBreadcrumbSchemaProvider CreateProvider(bool enabled, params TestContent[] ancestorsOrSelf)
        {
            var settingsService = new Mock<ISettingsService<MetaFieldsConfigModel>>();
            settingsService.Setup(x => x.GetSettings()).Returns(new MetaFieldsConfigModel { EnableBreadcrumbSchema = enabled });

            return new TestBreadcrumbSchemaProvider(settingsService.Object, ancestorsOrSelf);
        }

        private static TestContent CreateContent(string name, string url, int? templateId = 1)
        {
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.Name).Returns(name);
            content.Setup(x => x.ItemType).Returns(PublishedItemType.Content);
            content.Setup(x => x.TemplateId).Returns(templateId);
            return new TestContent(content.Object, url);
        }

        private record TestContent(IPublishedContent Content, string Url);

        private class TestBreadcrumbSchemaProvider : DefaultBreadcrumbSchemaProvider
        {
            private readonly TestContent[] _ancestorsOrSelf;

            public TestBreadcrumbSchemaProvider(ISettingsService<MetaFieldsConfigModel> settingsService, TestContent[] ancestorsOrSelf)
                : base(settingsService, Mock.Of<IDocumentNavigationQueryService>(), Mock.Of<IPublishedContentStatusFilteringService>())
            {
                _ancestorsOrSelf = ancestorsOrSelf;
            }

            public IThing? Get(TestContent content) => Get(content.Content);

            protected override IEnumerable<IPublishedContent> GetAncestorsOrSelf(IPublishedContent content)
                => _ancestorsOrSelf.Select(it => it.Content);

            protected override Uri? GetUrl(IPublishedContent content)
            {
                var url = _ancestorsOrSelf.First(it => it.Content == content).Url;
                return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
            }
        }
    }
}
