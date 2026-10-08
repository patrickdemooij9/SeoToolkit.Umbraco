using System.Text.Json;
using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SingleDropdownValueConverterTests
    {
        private readonly SingleDropdownValueConverter _converter = new();

        [Test]
        public void ConvertEditorToDatabaseValue_Selection_StoresTheItem()
        {
            var value = JsonDocument.Parse("[\"summary\"]").RootElement;
            Assert.That(_converter.ConvertEditorToDatabaseValue(value), Is.EqualTo("summary"));
        }

        [Test]
        public void ConvertEditorToDatabaseValue_ClearedSystemTextJson_StoresNull()
        {
            var value = JsonDocument.Parse("[]").RootElement;
            Assert.That(_converter.ConvertEditorToDatabaseValue(value), Is.Null);
        }

        [Test]
        public void ConvertEditorToDatabaseValue_ClearedNewtonsoft_StoresNull()
        {
            Assert.That(_converter.ConvertEditorToDatabaseValue(new JArray()), Is.Null);
        }

        [Test]
        public void ConvertEditorToDatabaseValue_PlainString_StoresIt()
        {
            Assert.That(_converter.ConvertEditorToDatabaseValue("player"), Is.EqualTo("player"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("[]")]
        [TestCase(" [] ")]
        public void SavedEmptyForms_ReadAsNoSelection(string databaseValue)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_converter.ConvertDatabaseToObject(databaseValue), Is.Null);
                Assert.That(_converter.IsEmpty(databaseValue), Is.True);
                Assert.That(_converter.ConvertObjectToEditorValue(databaseValue), Is.Empty);
            });
        }

        [Test]
        public void SavedSelection_ReadsBack()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_converter.ConvertDatabaseToObject("summary_large_image"), Is.EqualTo("summary_large_image"));
                Assert.That(_converter.IsEmpty("summary_large_image"), Is.False);
                Assert.That(_converter.ConvertObjectToEditorValue("summary_large_image"), Is.EqualTo(new[] { "summary_large_image" }));
            });
        }
    }
}
