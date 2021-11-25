using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels
{
    public class SeoSettingsFieldViewModel
    {
        public string Alias { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public object UserValue { get; set; }
        public string EditView { get; set; }
        public Dictionary<string, object> EditConfig { get; set; }
    }
}
