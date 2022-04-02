using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers
{
    public class BaseTagsFieldPreviewer : ISeoFieldPreviewer
    {
        public string Title => "Google";
        public string View => "~/App_Plugins/SeoToolkit/Interface/Previewers/BaseTags/baseTagsFieldPreviewer.html";
    }
}
