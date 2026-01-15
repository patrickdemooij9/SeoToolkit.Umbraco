using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels
{
    public class SeoSuggestionViewModel
    {
        public string Alias { get; set; }
        public Dictionary<string, object> Config { get; set; }
    }
}