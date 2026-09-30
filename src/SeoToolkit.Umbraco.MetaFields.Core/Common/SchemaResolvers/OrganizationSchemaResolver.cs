using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class OrganizationSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        public OrganizationSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Organization";
        public string Alias => "organization";

        public SchemaProperty[] Properties =>
            [
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("logo", "Logo", "Umb.PropertyEditorUi.MediaPicker", allowReference: false, valueConverter: _mediaConverter),
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("email", "Email", "Umb.PropertyEditorUi.TextBox"),
                new("telephone", "Telephone", "Umb.PropertyEditorUi.TextBox"),
                new("vatID", "Vat ID", "Umb.PropertyEditorUi.TextBox"),
                new("address", "Address", "SeoToolkit.SchemaEditor", allowReference: false, valueConverter: new SchemaEditorValueConverter(), config: new Dictionary<string, object>
                {
                    ["ownerType"] = "schemaEntry",
                    ["allowedSchemas"] = new[] { "postalAddress" }
                })
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            Uri.TryCreate(values.GetValueOrDefault("url")?.ToString(), UriKind.Absolute, out var url);
            Uri.TryCreate(values.GetValueOrDefault("logo")?.ToString(), UriKind.Absolute, out var logo);

            var addresses = values.GetValueOrDefault("address") as IThing[];
            return new Organization
            {
                Url = url,
                Logo = logo,
                Name = values.GetValueOrDefault("name")?.ToString(),
                Description = values.GetValueOrDefault("description")?.ToString(),
                Email = values.GetValueOrDefault("email")?.ToString(),
                Telephone = values.GetValueOrDefault("telephone")?.ToString(),
                VatID = values.GetValueOrDefault("vatID")?.ToString(),
                Address = addresses?.OfType<IPostalAddress>().ToArray() ?? []
            };
        }
    }
}
