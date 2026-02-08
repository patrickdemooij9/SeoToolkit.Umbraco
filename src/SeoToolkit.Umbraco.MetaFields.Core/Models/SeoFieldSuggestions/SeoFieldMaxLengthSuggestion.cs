using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldSuggestions
{
    public class SeoFieldMaxLengthSuggestion : ISeoFieldSuggestion
    {
        public string Alias => "maxLength";
        public int MaxLength { get; set; }

        public SeoSuggestionViewModel ToViewModel()
        {
            return new SeoSuggestionViewModel
            {
                Alias = Alias,
                Config = new Dictionary<string, object>
                {
                    { "maxLength", MaxLength }
                }
            };
        }
    }
}