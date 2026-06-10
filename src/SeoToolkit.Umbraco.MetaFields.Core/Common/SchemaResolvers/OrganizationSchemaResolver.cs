using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

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

        public IThing ToSchema(Dictionary<string, string> values)
        {
            Uri.TryCreate(values.GetValueOrDefault("url"), UriKind.Absolute, out var url);
            Uri.TryCreate(values.GetValueOrDefault("logo"), UriKind.Absolute, out var logo);

            return new Organization
            {
                Url = url,
                Logo = logo,
                Name = values.GetValueOrDefault("name"),
                Description = values.GetValueOrDefault("description"),
                Email = values.GetValueOrDefault("email"),
                Telephone = values.GetValueOrDefault("telephone"),
                VatID = values.GetValueOrDefault("vatID"),
            };
        }
    }
}
