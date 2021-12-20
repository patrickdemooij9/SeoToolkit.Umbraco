using System.Collections.Generic;
using Umbraco.Cms.Core.Web;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
{
    public class SeoImageEditEditor : ISeoFieldEditEditor
    {
        public string View => "MediaPicker";
        public Dictionary<string, object> Config { get; }
        public IEditorValueConverter ValueConverter { get; }

        public SeoImageEditEditor(IUmbracoContextFactory umbracoContextFactory)
        {
            ValueConverter = new UmbracoMediaUdiConverter(umbracoContextFactory);
            Config = new Dictionary<string, object>
            {
                {"disableFolderSelect", false},
                {"idType", "udi"},
                {"ignoreUserStartNodes", false},
                {"multiPicker", false},
                {"onlyImages", true}
            };
        }
    }
}
