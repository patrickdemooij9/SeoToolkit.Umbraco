using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers
{
    public class BaseTagsFieldPreviewer : ISeoFieldPreviewer
    {
        public string Title => "Google";
        public string View => "~/App_Plugins/uSeoToolkit/Interface/Previewers/BaseTags/baseTagsFieldPreviewer.html";
    }
}
