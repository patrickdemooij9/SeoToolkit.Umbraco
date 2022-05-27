using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers
{
    public class MetaFieldsPreviewer : ISeoFieldPreviewer
    {
        public string Group => SeoFieldGroupConstants.MetaFieldsGroup;
        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/MetaFields/metaFieldsPreviewer.html";
    }
}
