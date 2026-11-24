using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.Sitemap.Core.Collections;
using SeoToolkit.Umbraco.Sitemap.Core.Common.SitemapGenerators;
using SeoToolkit.Umbraco.Sitemap.Core.Config.Models;
using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SitemapGeneratorTests
    {
        private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private SitemapConfig _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = new SitemapConfig { MaxUrlsPerSitemap = 2, CacheDurationInMinutes = 0, ShowAlternatePages = false };
        }

        [Test]
        public void Generate_WithSingleCollectionWithinLimit_ReturnsUrlSet()
        {
            var generator = CreateGenerator(new TestProvider("news", 2));

            var doc = generator.Generate(CreateOptions());

            Assert.That(doc.Root!.Name, Is.EqualTo(Ns + "urlset"));
            Assert.That(doc.Root.Elements(Ns + "url"), Has.Exactly(2).Items);
        }

        [Test]
        public void Generate_WithDefaultLimit_DoesNotSplitLargeCollection()
        {
            _settings = new SitemapConfig { CacheDurationInMinutes = 0, ShowAlternatePages = false };
            var generator = CreateGenerator(new TestProvider("pages", 60000, new DateTime(2024, 1, 1)));

            var doc = generator.Generate(CreateOptions());

            Assert.That(_settings.MaxUrlsPerSitemap, Is.EqualTo(int.MaxValue));
            Assert.That(doc.Root!.Name, Is.EqualTo(Ns + "urlset"));
            Assert.That(doc.Root.Elements(Ns + "url"), Has.Exactly(60000).Items);
        }

        [Test]
        public void Generate_WithCollectionOverLimit_ReturnsIndexWithAllParts()
        {
            var generator = CreateGenerator(new TestProvider("news", 5));

            var doc = generator.Generate(CreateOptions());

            Assert.That(doc.Root!.Name, Is.EqualTo(Ns + "sitemapindex"));
            Assert.That(GetLocs(doc), Is.EqualTo(new[]
            {
                "https://example.com/news-1-sitemap.xml",
                "https://example.com/news-2-sitemap.xml",
                "https://example.com/news-3-sitemap.xml"
            }));
        }

        [Test]
        public void Generate_WithMultipleCollections_ListsEachCollectionSeparately()
        {
            var generator = CreateGenerator(new TestProvider("pages", 1), new TestProvider("products", 1));

            var doc = generator.Generate(CreateOptions());

            Assert.That(GetLocs(doc), Is.EqualTo(new[]
            {
                "https://example.com/pages-1-sitemap.xml",
                "https://example.com/products-1-sitemap.xml"
            }));
        }

        [Test]
        public void Generate_SitemapIndex_ContainsNewestLastModifiedOfPart()
        {
            _settings.LastModifiedFormat = "yyyy-MM-dd";
            var generator = CreateGenerator(new TestProvider("news", 3));

            var doc = generator.Generate(CreateOptions());

            var lastMods = doc.Root!.Elements(Ns + "sitemap").Select(it => it.Element(Ns + "lastmod")?.Value).ToArray();
            Assert.That(lastMods, Is.EqualTo(new[] { "2024-01-02", "2024-01-03" }));
        }

        [Test]
        public void GetParts_SkipsDisabledEmptyInvalidAndDuplicateCollections()
        {
            _settings.DisabledCollections = new[] { "disabled-by-config" };
            var generator = CreateGenerator(
                new TestProvider("pages", 1),
                new TestProvider("empty", 0),
                new TestProvider("not-enabled", 1) { Enabled = false },
                new TestProvider("disabled-by-config", 1),
                new TestProvider("Invalid Alias", 1),
                new TestProvider("pages", 1));

            var parts = generator.GetParts(CreateOptions());

            Assert.That(parts.Select(it => it.Alias), Is.EqualTo(new[] { "pages" }));
        }

        [Test]
        public void GetParts_ExcludesHiddenItems()
        {
            var provider = new TestProvider("news", 3);
            provider.Items[0].HideFromSitemap = true;
            var generator = CreateGenerator(provider);

            var parts = generator.GetParts(CreateOptions());

            Assert.That(parts.Sum(it => it.Items.Count), Is.EqualTo(2));
        }

        [Test]
        public void GetParts_PassesContextToCollection()
        {
            var provider = new TestProvider("news", 1);
            var generator = CreateGenerator(provider);

            generator.GetParts(new SitemapGeneratorOptions(null, "nl-NL", "https://base.com") { RootUrl = "https://example.com/" });

            Assert.That(provider.LastContext!.Culture, Is.EqualTo("nl-NL"));
            Assert.That(provider.LastContext.BaseUrl, Is.EqualTo("https://base.com"));
            Assert.That(provider.LastContext.RootNode, Is.Null);
        }

        [Test]
        public void GeneratePart_ReturnsOnlyItemsOfThatPart()
        {
            var generator = CreateGenerator(new TestProvider("news", 5));

            var doc = generator.GeneratePart(CreateOptions(), "news", 3);

            Assert.That(doc!.Root!.Name, Is.EqualTo(Ns + "urlset"));
            Assert.That(doc.Root.Elements(Ns + "url").Select(it => it.Element(Ns + "loc")!.Value), Is.EqualTo(new[] { "https://example.com/news/5" }));
        }

        [Test]
        public void GeneratePart_WithUnknownAliasOrIndex_ReturnsNull()
        {
            var generator = CreateGenerator(new TestProvider("news", 3));

            Assert.That(generator.GeneratePart(CreateOptions(), "news", 3), Is.Null);
            Assert.That(generator.GeneratePart(CreateOptions(), "other", 1), Is.Null);
        }

        [Test]
        public void GetParts_WithLegacyCollection_UsesParameterlessGetItems()
        {
            var generator = CreateGenerator(new LegacyProvider());

            var parts = generator.GetParts(CreateOptions());

            Assert.That(parts, Has.Exactly(1).Items);
            Assert.That(parts[0].Alias, Is.EqualTo("legacy"));
            Assert.That(parts[0].Url, Is.EqualTo("https://example.com/legacy-1-sitemap.xml"));
        }

        [Test]
        public void GetParts_WithCacheEnabled_CallsCollectionOnce()
        {
            _settings.CacheDurationInMinutes = 10;
            var provider = new TestProvider("news", 1);
            var appCaches = new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));
            var generator = CreateGenerator(appCaches, provider);

            generator.GetParts(CreateOptions());
            generator.GetParts(CreateOptions());

            Assert.That(provider.CallCount, Is.EqualTo(1));
        }

        [TestCase(typeof(NewsArticlesSitemapCollectionProvider), "news-articles")]
        [TestCase(typeof(LegacyProvider), "legacy")]
        [TestCase(typeof(Products2024), "products2024")]
        public void SitemapAliasHelper_FromType_ReturnsKebabCaseAlias(Type type, string expected)
        {
            Assert.That(SitemapAliasHelper.FromType(type), Is.EqualTo(expected));
            Assert.That(SitemapAliasHelper.IsValid(expected), Is.True);
        }

        private SitemapGenerator CreateGenerator(params ISitemapCollectionProvider[] providers)
            => CreateGenerator(AppCaches.NoCache, providers);

        private SitemapGenerator CreateGenerator(AppCaches appCaches, params ISitemapCollectionProvider[] providers)
        {
            var settingsService = new Mock<ISettingsService<SitemapConfig>>();
            settingsService.Setup(it => it.GetSettings()).Returns(_settings);

            return new SitemapGenerator(settingsService.Object,
                new Mock<IVariationContextAccessor>().Object,
                new SitemapCollectionProviderCollection(() => providers),
                appCaches,
                NullLogger<SitemapGenerator>.Instance);
        }

        private static SitemapGeneratorOptions CreateOptions()
            => new(null, "en-US") { RootUrl = "https://example.com/" };

        private static string[] GetLocs(XDocument doc)
            => doc.Root!.Elements(Ns + "sitemap").Select(it => it.Element(Ns + "loc")!.Value).ToArray();

        private class TestProvider : ISitemapCollectionProvider
        {
            public string Alias { get; }
            public bool Enabled { get; set; } = true;
            public List<SitemapNodeItem> Items { get; }
            public SitemapCollectionContext? LastContext { get; private set; }
            public int CallCount { get; private set; }

            public TestProvider(string alias, int count, DateTime? lastModifiedDate = null)
            {
                Alias = alias;
                Items = Enumerable.Range(1, count)
                    .Select(i => new SitemapNodeItem($"https://example.com/{alias}/{i}") { LastModifiedDate = lastModifiedDate ?? new DateTime(2024, 1, i) })
                    .ToList();
            }

            public bool IsEnabled(SitemapCollectionContext context) => Enabled;

            public IEnumerable<SitemapNodeItem> GetItems(SitemapCollectionContext context)
            {
                CallCount++;
                LastContext = context;
                return Items;
            }
        }

        private class LegacyProvider : ISitemapCollectionProvider
        {
            public SitemapNodeItem[] GetItems() => new[] { new SitemapNodeItem("https://example.com/legacy") };
        }

        private class NewsArticlesSitemapCollectionProvider : ISitemapCollectionProvider
        {
        }

        private class Products2024 : ISitemapCollectionProvider
        {
        }
    }
}
