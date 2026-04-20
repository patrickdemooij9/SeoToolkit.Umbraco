using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor
{
    public class SchemaEditorValueModel
    {
        public SchemaEditorModel[] Schemas { get; set; } = [];
    }

    public class SchemaEditorModel
    {
        public string SchemaAlias { get; set; }
        public Dictionary<string, SchemaPropertyValue> Properties { get; set; }
    }

    public class SchemaPropertyValue
    {
        public string Value { get; set; }
        public bool IsReference { get; set; }
        public string ReferenceKey { get; set; }
    }
}
