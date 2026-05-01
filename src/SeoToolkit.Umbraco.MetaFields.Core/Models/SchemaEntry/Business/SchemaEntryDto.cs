using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business
{
    public class SchemaEntryDto
    {
        public Guid Id { get; set; }
        public string OwnerType { get; set; }
        public Guid OwnerKey { get; set; }
        public string SchemaAlias { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, SchemaPropertyValue> Properties { get; set; } = new();
    }
}
