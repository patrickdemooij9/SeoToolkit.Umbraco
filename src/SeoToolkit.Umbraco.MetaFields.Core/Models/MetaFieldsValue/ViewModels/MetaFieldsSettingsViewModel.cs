using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsValue.ViewModels
{
    public class MetaFieldsSettingsViewModel
    {
        public SeoSettingsFieldViewModel[] Fields { get; set; }
        public SeoFieldGroupViewModel[] Groups { get; set; }
        public FieldPreviewerViewModel[] Previewers { get; set; }
    }
}
