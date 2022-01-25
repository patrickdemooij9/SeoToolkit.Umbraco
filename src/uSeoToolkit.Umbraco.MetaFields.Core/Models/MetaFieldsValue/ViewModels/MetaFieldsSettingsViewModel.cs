using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldPreviewers;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.ViewModels
{
    public class MetaFieldsSettingsViewModel
    {
        public SeoSettingsFieldViewModel[] Fields { get; set; }
        public FieldPreviewerViewModel[] Previewers { get; set; }
    }
}
