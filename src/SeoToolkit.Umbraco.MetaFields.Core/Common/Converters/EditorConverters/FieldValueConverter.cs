using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters
{
    public class FieldValueConverter : IEditorValueConverter
    {
        public object ConvertEditorToDatabaseValue(object value)
        {
            if (!(value is JArray fields))
                return null;

            return new FieldsModel
            {
                Fields = fields.ToObject<FieldsItem[]>()
            };
        }

        public object ConvertObjectToEditorValue(object value)
        {
            if (value is null || !(value is FieldsModel fieldModel))
                return Array.Empty<string>();

            return fieldModel.Fields ?? Array.Empty<FieldsItem>();
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
