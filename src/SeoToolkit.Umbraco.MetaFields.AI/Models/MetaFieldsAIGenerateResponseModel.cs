namespace SeoToolkit.Umbraco.MetaFields.AI.Models
{
    public class MetaFieldsAIFieldSuggestion
    {
        public string Alias { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class MetaFieldsAIGenerateResponseModel
    {
        public List<MetaFieldsAIFieldSuggestion> Suggestions { get; set; } = [];
    }
}
