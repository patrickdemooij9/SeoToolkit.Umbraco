using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels
{
    public class SeoFieldSettingsGroupViewModel
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }

        public SeoFieldSettingsGroupViewModel(ISeoFieldSettingsGroup group)
        {
            Alias = group.Alias;
            Name = group.Name;
            Description = group.Description;
            Icon = group.Icon;
        }
    }
}
