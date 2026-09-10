using NUnit.Framework;
using SeoToolkit.Umbraco.Deploy;

namespace SeoToolkit.Tests.Deploy
{
    [TestFixture]
    public class UdiJsonHelperTests
    {
        [Test]
        public void FindUdis_FindsDocumentAndMediaUdis_IgnoresDuplicatesAndGarbage()
        {
            var json = """
                {"image":"umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11",
                 "link":"umb://document/2b7e8d0aa4514f7e9d3c1e5f6a7b8c92",
                 "again":"umb://document/2b7e8d0aa4514f7e9d3c1e5f6a7b8c92",
                 "text":"not a udi umb://document/xyz"}
                """;

            var udis = UdiJsonHelper.FindUdis(json).ToArray();

            Assert.That(udis, Has.Length.EqualTo(2));
            Assert.That(udis.Select(u => u.ToString()), Does.Contain("umb://media/1c6f9c9df1c34b2c8a5a2d9a8e4b0c11"));
        }

        [Test]
        public void FindUdis_NullOrEmpty_ReturnsEmpty()
        {
            Assert.That(UdiJsonHelper.FindUdis(null), Is.Empty);
            Assert.That(UdiJsonHelper.FindUdis(""), Is.Empty);
        }
    }
}
