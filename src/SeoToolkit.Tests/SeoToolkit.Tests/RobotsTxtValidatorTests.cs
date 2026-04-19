using System.Linq;
using SeoToolkit.Umbraco.RobotsTxt.Core.Common.Validators;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class RobotsTxtValidatorTests
    {
        [Test]
        public void Validate_WhenDisallowAllForWildcardUserAgent_ReturnsWarning()
        {
            // Arrange
            var validator = new DefaultRobotsTxtValidator();
            var content = "User-agent: *\nDisallow: /";

            // Act
            var results = validator.Validate(content).ToArray();

            // Assert
            Assert.That(results.Any(it => it.Error == "You are currently blocking all bots which will impact your SEO. Make sure to check if this is correct."), Is.True);
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
            Assert.That(results.Any(it => it.Error == "You are currently blocking all bots which will impact your SEO. Make sure to check if this is correct."), Is.False);
        }
    }
}
