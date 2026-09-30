using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.PostModels
{
    public class SchemaEntryPostModel
    {
        public Guid? Id { get; set; }
        public string OwnerType { get; set; }
        public Guid OwnerKey { get; set; }
        public string SchemaAlias { get; set; }
        public string DisplayName { get; set; }
        public bool RenderAutomatically { get; set; } = true;
        public Dictionary<string, SchemaPropertyValue> Properties { get; set; } = new();
    }
}
