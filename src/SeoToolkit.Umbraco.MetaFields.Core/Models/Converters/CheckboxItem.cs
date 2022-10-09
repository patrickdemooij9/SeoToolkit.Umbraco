namespace SeoToolkit.Umbraco.MetaFields.Core.Models.Converters
{
    public class CheckboxItem
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public CheckboxItem(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }
}
