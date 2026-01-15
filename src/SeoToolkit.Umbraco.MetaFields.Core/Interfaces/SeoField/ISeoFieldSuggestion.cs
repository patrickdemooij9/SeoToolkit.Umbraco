using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldSuggestion
    {
        string Alias { get; }

        SeoSuggestionViewModel ToViewModel();
    }
}