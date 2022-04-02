using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.ViewModels
{
    public class MetaFieldsSettingsViewModel
    {
        public SeoSettingsFieldViewModel[] Fields { get; set; }
        public FieldPreviewerViewModel[] Previewers { get; set; }
    }
}
