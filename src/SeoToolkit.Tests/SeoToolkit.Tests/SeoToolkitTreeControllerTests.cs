using Moq;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.Common.Core.Models.Config;
using SeoToolkit.Umbraco.Common.Core.Services.Domains;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SeoToolkitTreeControllerTests
    {
        private Mock<SeoTreeSectionCollection> _mockSeoTreeSections = null!;
        private Mock<ISeoDomainsService> _mockSeoDomainsService = null!;
        private Mock<IDomainService> _mockDomainService = null!;
        private Mock<SeoKeyValueSettingCollection> _mockSeoKeyValueSettings = null!;
        private Mock<ISettingsService<GlobalConfig>> _mockConfig = null!;
        private SeoToolkitTreeController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _mockSeoTreeSections = new Mock<SeoTreeSectionCollection>(
                () => Array.Empty<ISeoTreeSection>());
            _mockSeoDomainsService = new Mock<ISeoDomainsService>();
            _mockDomainService = new Mock<IDomainService>();
            _mockSeoKeyValueSettings = new Mock<SeoKeyValueSettingCollection>(
                () => Array.Empty<ISeoKeyValueSetting>());
            _mockConfig = new Mock<ISettingsService<GlobalConfig>>();

            _controller = new SeoToolkitTreeController(
                _mockSeoTreeSections.Object,
                _mockSeoDomainsService.Object,
                _mockDomainService.Object,
                _mockSeoKeyValueSettings.Object,
                _mockConfig.Object);
        }

        [Test]
        public void FindMissingUmbracoDomains_GivenMultipleUrlTypes_OnlyShowsAbsoluteUrls()
        {
            // Arrange
            var seoDomains = new SeoDomainCollection[] { };

            var umbracoDomains = new List<IDomain>
            {
                CreateMockDomain(1, "https://example.com"),
                CreateMockDomain(2, "/en"),
                CreateMockDomain(3, "https://another-site.com"),
                CreateMockDomain(4, "/de"),
                CreateMockDomain(5, "https://third-site.com"),
                CreateMockDomain(6, "/fr"),
            };

            _mockDomainService
                .Setup(d => d.GetAllAsync(false))
                .ReturnsAsync(umbracoDomains.AsEnumerable());

            // Act
            var result = InvokeFindMissingUmbracoDomains(seoDomains);

            // Assert
            Assert.That(result, Has.Length.EqualTo(3), "Should only contain absolute URLs");
            Assert.That(result.All(d => !d.DomainName.StartsWith('/')), Is.True, "All returned domains should be absolute URLs");
            Assert.That(result.Select(d => d.DomainName), Does.Contain("https://example.com"));
            Assert.That(result.Select(d => d.DomainName), Does.Contain("https://another-site.com"));
            Assert.That(result.Select(d => d.DomainName), Does.Contain("https://third-site.com"));
        }

        [Test]
        public void FindMissingUmbracoDomains_GivenOnlyRelativeUrls_ReturnsEmpty()
        {
            // Arrange
            var seoDomains = new SeoDomainCollection[] { };

            var umbracoDomains = new List<IDomain>
            {
                CreateMockDomain(1, "/en"),
                CreateMockDomain(2, "/de"),
                CreateMockDomain(3, "/fr"),
            };

            _mockDomainService
                .Setup(d => d.GetAllAsync(false))
                .ReturnsAsync(umbracoDomains.AsEnumerable());

            // Act
            var result = InvokeFindMissingUmbracoDomains(seoDomains);

            // Assert
            Assert.That(result, Is.Empty, "Should return empty when only relative URLs are present");
        }

        [Test]
        public void FindMissingUmbracoDomains_GivenOnlyAbsoluteUrls_ReturnsAllUrls()
        {
            // Arrange
            var seoDomains = new SeoDomainCollection[] { };

            var umbracoDomains = new List<IDomain>
            {
                CreateMockDomain(1, "https://example.com"),
                CreateMockDomain(2, "https://another-site.com"),
                CreateMockDomain(3, "https://third-site.com"),
            };

            _mockDomainService
                .Setup(d => d.GetAllAsync(false))
                .ReturnsAsync(umbracoDomains.AsEnumerable());

            // Act
            var result = InvokeFindMissingUmbracoDomains(seoDomains);

            // Assert
            Assert.That(result, Has.Length.EqualTo(3), "Should return all absolute URLs");
        }

        [Test]
        public void FindMissingUmbracoDomains_GivenAlreadyUsedDomains_ExcludesThem()
        {
            // Arrange
            var domainId1 = 1;
            var domainId2 = 2;

            var seoDomain = new SeoDomainCollection { Name = "Test Domain" };
            seoDomain.DomainIds.Add(domainId1);

            var seoDomains = new[] { seoDomain };

            var umbracoDomains = new List<IDomain>
            {
                CreateMockDomain(domainId1, "https://used-domain.com"),
                CreateMockDomain(domainId2, "https://unused-domain.com"),
                CreateMockDomain(3, "/en"),
            };

            _mockDomainService
                .Setup(d => d.GetAllAsync(false))
                .ReturnsAsync(umbracoDomains.AsEnumerable());

            // Act
            var result = InvokeFindMissingUmbracoDomains(seoDomains);

            // Assert
            Assert.That(result, Has.Length.EqualTo(1), "Should exclude used domains");
            Assert.That(result.First().DomainName, Is.EqualTo("https://unused-domain.com"));
        }

        [Test]
        public void FindMissingUmbracoDomains_WithMixedDomainsAndUsedIds_ReturnCorrectResults()
        {
            // Arrange
            var usedDomainId = 1;

            var seoDomain = new SeoDomainCollection { Name = "Test Domain" };
            seoDomain.DomainIds.Add(usedDomainId);

            var seoDomains = new[] { seoDomain };

            var umbracoDomains = new List<IDomain>
            {
                CreateMockDomain(usedDomainId, "https://used-domain.com"),
                CreateMockDomain(2, "https://unused-absolute.com"),
                CreateMockDomain(3, "/en"),
                CreateMockDomain(4, "/de"),
                CreateMockDomain(5, "https://another-unused-absolute.com"),
            };

            _mockDomainService
                .Setup(d => d.GetAllAsync(false))
                .ReturnsAsync(umbracoDomains.AsEnumerable());

            // Act
            var result = InvokeFindMissingUmbracoDomains(seoDomains);

            // Assert
            Assert.That(result, Has.Length.EqualTo(2), "Should return only unused absolute URLs");
            var returnedDomains = result.Select(d => d.DomainName).ToList();
            Assert.That(returnedDomains, Does.Contain("https://unused-absolute.com"));
            Assert.That(returnedDomains, Does.Contain("https://another-unused-absolute.com"));
        }

        private IDomain[] InvokeFindMissingUmbracoDomains(SeoDomainCollection[] seoDomains)
        {
            // Use reflection to invoke the private method
            var method = typeof(SeoToolkitTreeController).GetMethod(
                "FindMissingUmbracoDomains",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method == null)
                throw new InvalidOperationException("FindMissingUmbracoDomains method not found");

            var task = (Task<IDomain[]>)method.Invoke(_controller, new object[] { seoDomains })!;
            return task.Result;
        }

        private IDomain CreateMockDomain(int id, string domainName)
        {
            var mockDomain = new Mock<IDomain>();
            mockDomain.Setup(d => d.Id).Returns(id);
            mockDomain.Setup(d => d.DomainName).Returns(domainName);
            return mockDomain.Object;
        }
    }
}
