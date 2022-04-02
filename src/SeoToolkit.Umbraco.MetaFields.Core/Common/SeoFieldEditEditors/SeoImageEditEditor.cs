using System.Collections.Generic;
using Umbraco.Cms.Core.Web;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldEditEditors
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
