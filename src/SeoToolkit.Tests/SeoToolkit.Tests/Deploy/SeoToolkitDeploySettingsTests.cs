using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using SeoToolkit.Umbraco.Deploy.Configuration;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SeoToolkitDeploySettingsTests
    {
        private static IEnumerable<string> AllEntityTypes()
            => typeof(SeoToolkitDeployConstants.UdiEntityType)
                .GetFields()
                .Select(f => (string)f.GetValue(null)!);

        [Test]
        public void Defaults_EnableEverythingExceptScripts()
        {
            var settings = new SeoToolkitDeploySettings();

            foreach (var entityType in AllEntityTypes())
            {
                Assert.That(settings.IsEnabled(entityType), Is.EqualTo(entityType != SeoToolkitDeployConstants.UdiEntityType.Script), entityType);
            }
        }

        [Test]
        public void EveryEntityType_CanBeToggledFromConfiguration()
        {
            var sections = new[]
            {
                "SeoSettings", "MetaFieldsSettings", "MetaFieldsValues", "SitemapPageTypes",
                "SitemapContent", "Scripts", "DomainCollections", "KeyValues",
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(sections.ToDictionary(s => $"SeoToolkit:Deploy:{s}:Enabled", s => (string?)(s == "Scripts" ? "true" : "false")))
                .Build();

            var settings = config.GetSection("SeoToolkit:Deploy").Get<SeoToolkitDeploySettings>()!;

            foreach (var entityType in AllEntityTypes())
            {
                Assert.That(settings.IsEnabled(entityType), Is.EqualTo(entityType == SeoToolkitDeployConstants.UdiEntityType.Script), entityType);
            }
        }
    }
}
