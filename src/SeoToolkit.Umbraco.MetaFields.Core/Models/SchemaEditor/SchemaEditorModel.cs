using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor
{
    public class SchemaEditorValueModel
    {
        public string[] Schemas { get; set; } = [];
    }

    public class SchemaEditorModel
    {
        public string SchemaAlias { get; set; }
        public Dictionary<string, SchemaPropertyValue> Properties { get; set; }
    }

    public class SchemaPropertyValue
    {
        public object Value { get; set; }
        public bool IsReference { get; set; }
        public string ReferenceKey { get; set; }
    }
}
