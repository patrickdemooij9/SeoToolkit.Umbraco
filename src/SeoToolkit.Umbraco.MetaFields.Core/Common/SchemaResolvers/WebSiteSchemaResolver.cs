using Schema.NET;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class WebSiteSchemaResolver : ISchemaResolver
    {
        public string Name => "Website";
        public string Alias => "webSite";

        public SchemaProperty[] Properties =>
            [
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("alternateName", "Alternate Name", "Umb.PropertyEditorUi.TextBox"),
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("inLanguage", "Language", "Umb.PropertyEditorUi.TextBox"),
                SchemaValueHelper.NestedSchemaProperty("publisher", "Publisher", "organization", "localBusiness", "person")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            return new WebSite
            {
                Name = values.GetString("name"),
                AlternateName = values.GetString("alternateName"),
                Url = values.GetUri("url"),
                Description = values.GetString("description"),
                InLanguage = values.GetString("inLanguage"),
                Publisher = values.GetOrganizationsOrPersons("publisher")
            };
        }
    }
}
