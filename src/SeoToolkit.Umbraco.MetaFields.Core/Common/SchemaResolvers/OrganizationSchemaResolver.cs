using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class OrganizationSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;
        private readonly TextValueConverter _textConverter = new TextValueConverter();

        public OrganizationSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Organization";
        public string Alias => "organization";

        public SchemaProperty[] Properties =>
            [
                new("url", "Url", "Umb.PropertyEditorUi.TextBox", valueConverter: _textConverter),
                new("logo", "Logo", "Umb.PropertyEditorUi.MediaPicker", allowReference: false, valueConverter: _mediaConverter),
                new("name", "Name", "Umb.PropertyEditorUi.TextBox", valueConverter: _textConverter),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea", valueConverter: _textConverter),
                new("email", "Email", "Umb.PropertyEditorUi.TextBox", valueConverter: _textConverter),
                new("telephone", "Telephone", "Umb.PropertyEditorUi.TextBox", valueConverter: _textConverter),
                new("vatID", "Vat ID", "Umb.PropertyEditorUi.TextBox", valueConverter: _textConverter)
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
