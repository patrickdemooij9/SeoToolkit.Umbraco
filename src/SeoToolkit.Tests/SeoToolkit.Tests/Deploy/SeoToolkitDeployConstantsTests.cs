using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;
using Umbraco.Cms.Core.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class SeoToolkitDeployConstantsTests
    {
        [Test]
        public void UdiEntityTypes_AreAllSeoToolkitPrefixed()
        {
            var all = new[]
            {
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting,
                SeoToolkitDeployConstants.UdiEntityType.SitemapPageType,
                SeoToolkitDeployConstants.UdiEntityType.Script,
                SeoToolkitDeployConstants.UdiEntityType.DomainCollection,
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue,
                SeoToolkitDeployConstants.UdiEntityType.SitemapContent,
            };
            Assert.That(all, Is.Unique);
            Assert.That(all, Is.All.Matches<string>(s => s.StartsWith("seotoolkit-")));
        }

        [Test]
        public void SeoToolkitArtifactDependency_DefaultsToExistNotOrdering()
        {
            var udi = new global::Umbraco.Cms.Core.GuidUdi(
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting, System.Guid.NewGuid());
            var dep = new SeoToolkitArtifactDependency(udi);
            Assert.Multiple(() =>
            {
                Assert.That(dep.Mode, Is.EqualTo(ArtifactDependencyMode.Exist));
                Assert.That(dep.Ordering, Is.False);
            });
        }
    }
}
