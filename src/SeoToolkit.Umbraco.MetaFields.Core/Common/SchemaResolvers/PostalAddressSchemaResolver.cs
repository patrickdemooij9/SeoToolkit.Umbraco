using Schema.NET;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class PostalAddressSchemaResolver : ISchemaResolver
    {
        public string Name => "Postal Address";

        public string Alias => "postalAddress";

        public SchemaProperty[] Properties => new[]
        {
            new SchemaProperty("streetAddress", "Street Address", "Umb.PropertyEditorUi.TextBox"),
            new SchemaProperty("addressLocality", "Address Locality", "Umb.PropertyEditorUi.TextBox"),
            new SchemaProperty("addressCountry", "Address Region", "Umb.PropertyEditorUi.TextBox"),
            new SchemaProperty("addressRegion", "Address Region", "Umb.PropertyEditorUi.TextBox"),
            new SchemaProperty("postalCode", "Postal Code", "Umb.PropertyEditorUi.TextBox"),
        };

        public IThing ToSchema(Dictionary<string, string> values)
        {
            return new PostalAddress
            {
                StreetAddress = values.GetValueOrDefault("streetAddress"),
                AddressLocality = values.GetValueOrDefault("addressLocality"),
                AddressCountry = values.GetValueOrDefault("addressCountry"),
                AddressRegion = values.GetValueOrDefault("addressRegion"),
                PostalCode = values.GetValueOrDefault("postalCode"),
            };
        }
    }
}
