using System;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class FieldValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            var fields = JsonHelpers.DeserializeArray<FieldsItem>(value);
            if (fields is null)
                return null;

            return new FieldsModel
            {
                Fields = fields
            };
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (value is null || value is not FieldsModel fieldModel)
                return Array.Empty<string>();

            return fieldModel.Fields ?? [];
        }

        public object ConvertDatabaseToObject(object value)
        {
            if (value is JObject jsonObject)
            {
                return jsonObject.ToObject<FieldsModel>();
            }

            return null;
        }

        public bool IsEmpty(object value)
        {
            return value is null || (value as FieldsModel)?.Fields?.Any() != true;
        }
    }
}
