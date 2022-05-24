using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers
{
    public class FieldPreviewerViewModel
    {
        public string Group { get; set; }
        public string View { get; set; }

        public FieldPreviewerViewModel(ISeoFieldPreviewer model)
        {
            Group = model.Group;
            View = model.View;
        }
    }
}
