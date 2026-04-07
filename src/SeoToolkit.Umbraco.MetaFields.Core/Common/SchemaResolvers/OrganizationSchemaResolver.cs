using Schema.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class OrganizationSchemaResolver : ISchemaResolver
    {
        public string Name => "Organization";
        public string Alias => "organization";

        public SchemaProperty[] Properties =>
            [
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("logo", "Logo", "Umb.PropertyEditorUi.MediaPicker"),
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("email", "Email", "Umb.PropertyEditorUi.TextBox"),
                new("telephone", "Telephone", "Umb.PropertyEditorUi.TextBox"),
                new("vatID", "Vat ID", "Umb.PropertyEditorUi.TextBox")
            ];

        public IThing ToSchema(Dictionary<string, string> values)
        {
            return new Organization
            {
                Url = new Uri(values["url"]),
                Logo = new Uri(values["logo"]),
                Name = values["name"],
                Description = values["description"],
                Email = values["email"],
                Telephone = values["telephone"],
                VatID = values["vatID"],
            };
        }
    }
}