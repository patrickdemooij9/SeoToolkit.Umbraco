using Moq;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Services;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RedirectHitTrackerTests
    {
        private Mock<IRedirectsRepository> _mockRedirectsRepository = null!;
        private RedirectHitTracker _tracker = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRedirectsRepository = new Mock<IRedirectsRepository>();
            _tracker = new RedirectHitTracker(_mockRedirectsRepository.Object);
        }

        [Test]
        public void Flush_WithRegisteredHits_UpdatesEachRedirectOnce()
        {
            // Arrange
            var firstKey = Guid.NewGuid();
            var secondKey = Guid.NewGuid();
            _tracker.RegisterHit(firstKey);
            _tracker.RegisterHit(firstKey);
            _tracker.RegisterHit(secondKey);

            // Act
            _tracker.Flush();

            // Assert
            _mockRedirectsRepository.Verify(it => it.UpdateLastHits(It.Is<IDictionary<Guid, DateTime>>(hits =>
                hits.Count == 2 && hits.ContainsKey(firstKey) && hits.ContainsKey(secondKey))), Times.Once);
        }

        [Test]
        public void Flush_WithRegisteredHit_UsesUtcTime()
        {
            // Arrange
            var key = Guid.NewGuid();
            _tracker.RegisterHit(key);

            // Act
            _tracker.Flush();

            // Assert
            _mockRedirectsRepository.Verify(it => it.UpdateLastHits(It.Is<IDictionary<Guid, DateTime>>(hits =>
                hits[key].Kind == DateTimeKind.Utc)), Times.Once);
        }

        [Test]
        public void Flush_AfterPreviousFlush_DoesNotUpdateSameHitsAgain()
        {
            // Arrange
            _tracker.RegisterHit(Guid.NewGuid());
            _tracker.Flush();

            // Act
            _tracker.Flush();

            // Assert
            _mockRedirectsRepository.Verify(it => it.UpdateLastHits(It.IsAny<IDictionary<Guid, DateTime>>()), Times.Once);
        }

        [Test]
        public void Flush_WithoutHits_DoesNotUpdate()
        {
            // Act
            _tracker.Flush();

            // Assert
            _mockRedirectsRepository.Verify(it => it.UpdateLastHits(It.IsAny<IDictionary<Guid, DateTime>>()), Times.Never);
        }
    }
}
