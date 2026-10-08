using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class SingleDropdownValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            // The editor sends its selection as an array, and an empty one is a cleared field. Don't
            // fall back to value.ToString() for it: that stores the text "[]" as the selection.
            var items = JsonHelpers.DeserializeArray<string>(value);
            return items is not null ? Selection(items.FirstOrDefault()) : Selection(value?.ToString());
        }

        public object ConvertObjectToEditorValue(object value)
        {
            var selection = Selection(value?.ToString());
            return selection is null ? Array.Empty<string>() : new[] { selection };
        }

        public object ConvertDatabaseToObject(object value)
        {
            return Selection(value?.ToString());
        }

        public bool IsEmpty(object value)
        {
            return Selection(value?.ToString()) is null;
        }

        // A cleared field used to be saved as the text "[]", so read that as no selection too: those
        // values then fall back to the document type's instead of rendering "[]".
        private static string Selection(string value)
            => string.IsNullOrWhiteSpace(value) || value.Trim() == "[]" ? null : value;
    }
}
