namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class SchemaProperty
    {
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public string PropertyEditor { get; set; }

        public SchemaProperty(string alias, string displayName, string propertyEditor)
        {
            Alias = alias;
            DisplayName = displayName;
            PropertyEditor = propertyEditor;
        }
    }
}
