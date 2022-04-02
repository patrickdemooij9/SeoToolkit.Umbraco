using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers
{
    public class FieldPreviewerViewModel
    {
        public string Title { get; set; }
        public string View { get; set; }

        public FieldPreviewerViewModel(ISeoFieldPreviewer model)
        {
            Title = model.Title;
            View = model.View;
        }
    }
}
