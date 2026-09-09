using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Models.Database;
using SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels;
using SeoToolkit.Umbraco.Redirects.Core.Repositories;
using System.Reflection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SeoToolkit.Tests
{
    /// <summary>
    /// A redirect only stores the key of the node it points to, so content and media items can only be
    /// told apart by looking the key up in the navigation structure. These tests guard that we never go
    /// back to deriving the node type from the culture, which breaks for content that doesn't vary by
    /// culture (issue #560), and that we never ask the document cache about a media key, which returns
    /// the media item's cache entry or poisons the next media lookup.
    /// </summary>
    [TestFixture]
    public class RedirectMappingTests
    {
        private delegate bool TryGetParentKeyCallback(Guid childKey, out Guid? parentKey);

        [Test]
        public void ToModel_ResolvesContentNode_WhenCultureIsNull()
        {
            // Arrange
            var nodeKey = Guid.NewGuid();
            var content = Mock.Of<IPublishedContent>();
            var repository = GetRepository(documentKey: nodeKey, content: content, mediaKey: null, media: null,
                out var contentCache, out var mediaCache);
            var entity = GetEntity(nodeKey, newNodeCultureId: null);

            // Act
            var redirect = ToModel(repository, entity);

            // Assert
            Assert.That(redirect.NewNode, Is.SameAs(content), "A redirect to a content node without a culture should still resolve the content node");
            Assert.That(redirect.NewNodeCulture, Is.Null);
            mediaCache.Verify(it => it.GetById(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void ToModel_ResolvesMediaNode_WithoutAskingTheDocumentCache()
        {
            // Arrange
            var nodeKey = Guid.NewGuid();
            var media = Mock.Of<IPublishedContent>();
            var repository = GetRepository(documentKey: null, content: null, mediaKey: nodeKey, media: media,
                out var contentCache, out var mediaCache);
            var entity = GetEntity(nodeKey, newNodeCultureId: null);

            // Act
            var redirect = ToModel(repository, entity);

            // Assert
            Assert.That(redirect.NewNode, Is.SameAs(media));
            // Both caches share one HybridCache and key format, and the document cache doesn't check that
            // the key is a document - asking it about a media key breaks the media item's own cache entry.
            contentCache.Verify(it => it.GetById(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public void ToModel_ResolvesContentNodeAndCulture_WhenCultureIsSet()
        {
            // Arrange
            var nodeKey = Guid.NewGuid();
            var content = Mock.Of<IPublishedContent>();
            var language = Mock.Of<ILanguage>(it => it.Id == 5 && it.IsoCode == "en-US");
            var repository = GetRepository(documentKey: nodeKey, content: content, mediaKey: null, media: null,
                out var contentCache, out var mediaCache, language: language);
            var entity = GetEntity(nodeKey, newNodeCultureId: 5);

            // Act
            var redirect = ToModel(repository, entity);

            // Assert
            Assert.That(redirect.NewNode, Is.SameAs(content));
            Assert.That(redirect.NewNodeCulture, Is.SameAs(language));
        }

        [TestCase(PublishedItemType.Content, "Content")]
        [TestCase(PublishedItemType.Media, "Media")]
        public void ViewModel_ExposesNodeType_SoTheUiDoesNotHaveToGuessItFromTheCulture(PublishedItemType itemType, string expectedNodeType)
        {
            // Arrange
            var redirect = new Redirect
            {
                Key = Guid.NewGuid(),
                OldUrl = "/old",
                NewNode = Mock.Of<IPublishedContent>(it => it.Key == Guid.NewGuid() && it.ItemType == itemType),
                NewNodeCulture = null,
                RedirectCode = 301
            };

            // Act
            var viewModel = new RedirectViewModel(redirect);

            // Assert
            Assert.That(viewModel.NewNodeType, Is.EqualTo(expectedNodeType));
        }

        private static Redirect ToModel(RedirectsRepository repository, RedirectEntity entity)
        {
            var toModel = typeof(RedirectsRepository).GetMethod("ToModel", BindingFlags.Instance | BindingFlags.NonPublic)!;
            return (Redirect)toModel.Invoke(repository, new object[] { entity })!;
        }

        private static RedirectEntity GetEntity(Guid nodeKey, int? newNodeCultureId)
        {
            return new RedirectEntity
            {
                Id = 1,
                Key = Guid.NewGuid(),
                IsEnabled = true,
                OldUrl = "/old",
                NewNodeKey = nodeKey,
                NewNodeCultureId = newNodeCultureId,
                RedirectCode = 301
            };
        }

        private static RedirectsRepository GetRepository(Guid? documentKey,
            IPublishedContent? content,
            Guid? mediaKey,
            IPublishedContent? media,
            out Mock<IPublishedContentCache> contentCache,
            out Mock<IPublishedMediaCache> mediaCache,
            ILanguage? language = null)
        {
            contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(it => it.GetById(It.IsAny<Guid>()))
                .Returns((Guid key) => key == documentKey ? content : null);
            mediaCache = new Mock<IPublishedMediaCache>();
            mediaCache.Setup(it => it.GetById(It.IsAny<Guid>()))
                .Returns((Guid key) => key == mediaKey ? media : null);

            var umbracoContext = new Mock<IUmbracoContext>();
            umbracoContext.Setup(it => it.Content).Returns(contentCache.Object);
            umbracoContext.Setup(it => it.Media).Returns(mediaCache.Object);

            var contextFactory = new Mock<IUmbracoContextFactory>();
            contextFactory.Setup(it => it.EnsureUmbracoContext())
                .Returns(() => new UmbracoContextReference(umbracoContext.Object, true, Mock.Of<IUmbracoContextAccessor>()));

            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(it => it.GetLanguageById(It.IsAny<int>()))
                .Returns((int id) => language?.Id == id ? language : null);

            // Only documents are part of the document navigation structure
            var expectedDocumentKey = documentKey;
            var documentNavigationQueryService = new Mock<IDocumentNavigationQueryService>();
            documentNavigationQueryService
                .Setup(it => it.TryGetParentKey(It.IsAny<Guid>(), out It.Ref<Guid?>.IsAny))
                .Returns(new TryGetParentKeyCallback((Guid childKey, out Guid? parentKey) =>
                {
                    parentKey = null;
                    return childKey == expectedDocumentKey;
                }));

            return new RedirectsRepository(Mock.Of<IScopeProvider>(),
                contextFactory.Object,
                localizationService.Object,
                AppCaches.Disabled,
                null!,
                Mock.Of<ILanguageService>(),
                documentNavigationQueryService.Object);
        }
    }
}
