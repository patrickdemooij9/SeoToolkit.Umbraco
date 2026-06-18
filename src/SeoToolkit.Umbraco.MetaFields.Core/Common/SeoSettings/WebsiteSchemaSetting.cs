using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoSettings
{
    public class WebsiteSchemaSetting : ISeoKeyValueSetting
    {
        public string Key => "websiteSchemas";

        public string Title => "Website schemas";

        public string Description => "Schemas that apply to the entire website. Schemas marked to render show on every page, and all of them can be referenced from document types and content.";

        public string PropertyAlias => "SeoToolkit.SchemaEditor";

        public Type EditorType => typeof(string);

        public IReadOnlyDictionary<string, object> EditConfig => new Dictionary<string, object>
        {
            ["ownerType"] = SchemaOwnerTypeConstants.Website,
            ["nodeGuid"] = SchemaOwnerTypeConstants.WebsiteOwnerKey.ToString()
        };

        // Website schemas are a single global collection, so only show them on the root node.
        public bool RootOnly => true;
    }
}
