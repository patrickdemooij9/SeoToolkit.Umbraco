using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RobotsTxtValidatorTests
    {
        private const string DisallowAllWarning = "You are currently blocking all bots which will impact your SEO. Make sure to check if this is correct.";

        [Test]
        public void Validate_WhenDisallowAllForWildcardUserAgent_ReturnsWarning()
        {
            // Arrange
            var validator = new DefaultRobotsTxtValidator();
            var content = "User-agent: *\nDisallow: /";

            // Act
            var results = validator.Validate(content).ToArray();

            // Assert
            Assert.That(results.Any(it => it.Error == DisallowAllWarning), Is.True);
        }

        [Test]
        public void Validate_WhenDisallowAllForSpecificUserAgent_DoesNotReturnWarning()
        {
            // Arrange
            var validator = new DefaultRobotsTxtValidator();
            var content = "User-agent: Googlebot\nDisallow: /";

            // Act
            var results = validator.Validate(content).ToArray();

            // Assert
            Assert.That(results.Any(it => it.Error == DisallowAllWarning), Is.False);
        }

        [Test]
        public void Validate_WhenMultipleDisallowAllInWildcardBlock_ReturnsSingleWarning()
        {
            // Arrange
            var validator = new DefaultRobotsTxtValidator();
            var content = "User-agent: *\nDisallow: /\nDisallow: /";

            // Act
            var results = validator.Validate(content).ToArray();

            // Assert
            Assert.That(results.Count(it => it.Error == DisallowAllWarning), Is.EqualTo(1));
        }

        [Test]
        public void Validate_WhenWildcardBlockFollowedBySpecificUserAgent_WarnsOnlyForWildcardBlock()
        {
            // Arrange
            var validator = new DefaultRobotsTxtValidator();
            var content = "User-agent: *\nDisallow: /\nUser-agent: Googlebot\nDisallow: /";

            // Act
            var results = validator.Validate(content).ToArray();

            // Assert
            Assert.That(results.Count(it => it.Error == DisallowAllWarning), Is.EqualTo(1));
        }
    }
}
