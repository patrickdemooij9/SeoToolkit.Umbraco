using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoFieldSuggestions
{
    public class SeoFieldMaxLengthSuggestion : ISeoFieldSuggestion
    {
        public string Alias => "maxLength";
        public int MaxLength { get; set; }

        /// <summary>
        /// When true, a value taken from the document type fallback is cut to <see cref="MaxLength"/>. A value an editor typed is never cut.
        /// </summary>
        public bool TruncateFallbackValue { get; set; }

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