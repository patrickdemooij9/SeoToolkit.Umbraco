using uSeoToolkit.Umbraco.MetaFields.Core.Enums;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters
{
    public class FieldsItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public FieldSourceType Source { get; set; }
    }
}
