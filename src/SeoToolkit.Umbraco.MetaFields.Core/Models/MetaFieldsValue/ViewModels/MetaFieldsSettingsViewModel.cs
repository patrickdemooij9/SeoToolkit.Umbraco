using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.MetaFieldsValue.ViewModels
{
    public class MetaFieldsSettingsViewModel
    {
        public SeoSettingsFieldViewModel[] Fields { get; set; }
        public SeoFieldGroupViewModel[] Groups { get; set; }
        public FieldPreviewerViewModel[] Previewers { get; set; }
    }
}
