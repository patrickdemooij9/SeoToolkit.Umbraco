using Newtonsoft.Json;
using NUnit.Framework;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Database;

namespace SeoToolkit.Tests.Deploy
{
    /// <summary>
    /// Models the full metafields-setting deploy value round-trip (import -> DB serialize/deserialize
    /// -> export) using the real editor value converters and the actual field values that appear in
    /// the committed .uda files, and asserts it converges (re-exported value == source value).
    /// A field that fails here is one whose entity would be re-flagged as changed on every deploy.
    /// </summary>
    [TestFixture]
    public class MetaFieldsSettingConvergenceTests
    {
        // Replicates SeoToolkitMetaFieldsSettingServiceConnector.ProcessAsync (import).
        private static object? Import(IEditorValueConverter converter, string? fieldValue)
            => string.IsNullOrWhiteSpace(fieldValue)
                ? null
                : converter.ConvertEditorToDatabaseValue(JsonConvert.DeserializeObject(fieldValue));

        // Replicates DocumentTypeSettingsMapper: DTO -> entity (Newtonsoft serialize, Value is object)
        // then entity -> DTO on read.
        private static object? ThroughDb(object? databaseForm)
        {
            var json = JsonConvert.SerializeObject(new[]
            {
                new MetaFieldsFieldEntity { Alias = "x", Value = databaseForm, UseInheritedValue = false },
            });
            var back = JsonConvert.DeserializeObject<MetaFieldsFieldEntity[]>(json);
            return back![0].Value;
        }

        // Replicates SeoToolkitMetaFieldsSettingServiceConnector.GetArtifactAsync (export).
        private static string? Export(IEditorValueConverter converter, object? databaseStored)
        {
            var objectForm = converter.ConvertDatabaseToObject(databaseStored);
            var editorValue = objectForm is null ? null : converter.ConvertObjectToEditorValue(objectForm);
            return editorValue is null ? null : JsonConvert.SerializeObject(editorValue);
        }

        private static string? RoundTrip(IEditorValueConverter converter, string? sourceValue)
            => Export(converter, ThroughDb(Import(converter, sourceValue)));

        [TestCase("\"%CurrentUrl%\"")]
        public void Text_Converges(string sourceValue)
            => Assert.That(RoundTrip(new TextValueConverter(), sourceValue), Is.EqualTo(sourceValue));

        [TestCase("[\"summary_large_image\"]")]
        [TestCase("[\"[]\"]")]
        public void List_Converges(string sourceValue)
            => Assert.That(RoundTrip(new ListValueConverter(), sourceValue), Is.EqualTo(sourceValue));

        [TestCase("[]")]
        [TestCase("[{\"Name\":\"Title\",\"Value\":\"pageTitle\",\"Source\":1},{\"Name\":\"Page Name\",\"Value\":\"custom-pageName\",\"Source\":2}]")]
        [TestCase("[{\"Name\":\"Banner Image\",\"Value\":\"bannerImage\",\"Source\":1}]")]
        public void Field_Converges(string sourceValue)
            => Assert.That(RoundTrip(new FieldValueConverter(), sourceValue), Is.EqualTo(sourceValue));

        // twitterCardType (via DropdownFieldPropertyEditor).
        [TestCase("[\"summary\"]")]
        [TestCase("[\"summary_large_image\"]")]
        [TestCase("[\"[]\"]")]
        public void SingleDropdown_Converges(string sourceValue)
            => Assert.That(RoundTrip(new SingleDropdownValueConverter(), sourceValue), Is.EqualTo(sourceValue));

        // Regression: ListValueConverter (the keywords field) must never yield null from
        // ConvertDatabaseToObject, so an empty keywords field always exports as "[]" (never omitted)
        // regardless of whether the DB holds null or an empty string — the two used to diverge and
        // re-flag the entity as changed on every deploy.
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("[]")]
        public void List_EmptyForms_ExportAsEmptyArray(string? databaseValue)
        {
            var converter = new ListValueConverter();
            var objectForm = converter.ConvertDatabaseToObject(databaseValue);
            Assert.That(objectForm, Is.Not.Null);
            Assert.That(Newtonsoft.Json.JsonConvert.SerializeObject(converter.ConvertObjectToEditorValue(objectForm)),
                Is.EqualTo("[]"));
        }

        // robots (via CheckboxListFieldPropertyEditor).
        [TestCase("[\"index\",\"follow\"]")]
        [TestCase("[\"[]\"]")]
        public void Checkboxlist_Converges(string sourceValue)
            => Assert.That(RoundTrip(new CheckboxlistConverter(), sourceValue), Is.EqualTo(sourceValue));

        // An omitted (null) source value must round-trip back to null, else it re-serialises with a
        // value and never converges. ListValueConverter is excluded: it always materialises an empty
        // array (exported as "[]"), so it is never omitted and this invariant does not apply.
        [Test]
        public void OmittedValue_StaysOmitted(
            [Values] ConverterKind kind)
        {
            IEditorValueConverter converter = kind switch
            {
                ConverterKind.Text => new TextValueConverter(),
                ConverterKind.Field => new FieldValueConverter(),
                ConverterKind.SingleDropdown => new SingleDropdownValueConverter(),
                _ => new CheckboxlistConverter(),
            };

            Assert.That(RoundTrip(converter, null), Is.Null);
        }

        public enum ConverterKind { Text, Field, SingleDropdown, Checkboxlist }
    }
}
