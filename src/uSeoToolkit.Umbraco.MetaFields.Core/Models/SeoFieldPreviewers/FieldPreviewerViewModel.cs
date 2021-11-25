using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers
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
