namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor
{
    public class SchemaTypeViewModel
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public SchemaPropertyViewModel[] Properties { get; set; }
    }

    public class SchemaPropertyViewModel
    {
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public string PropertyEditor { get; set; }
        public bool AllowReference { get; set; }
    }
}