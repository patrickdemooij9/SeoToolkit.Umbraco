using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class PersonSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        public PersonSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Person";
        public string Alias => "person";

        public SchemaProperty[] Properties =>
            [
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("image", "Image", "Umb.PropertyEditorUi.MediaPicker", valueConverter: _mediaConverter),
                new("jobTitle", "Job Title", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("email", "Email", "Umb.PropertyEditorUi.TextBox"),
                new("telephone", "Telephone", "Umb.PropertyEditorUi.TextBox"),
                new("sameAs", "Same As (one URL per line)", "Umb.PropertyEditorUi.TextArea"),
                SchemaValueHelper.NestedSchemaProperty("worksFor", "Works For", "organization", "localBusiness"),
                SchemaValueHelper.NestedSchemaProperty("address", "Address", "postalAddress")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var image = values.GetUri("image");

            return new ExtendedPerson
            {
                Name = values.GetString("name"),
                Url = values.GetUri("url"),
                Image = image is null ? default : new Values<IImageObject, Uri>(image),
                JobTitle = values.GetString("jobTitle"),
                Description = values.GetString("description"),
                Email = values.GetString("email"),
                Telephone = values.GetString("telephone"),
                SameAs = values.GetUris("sameAs"),
                WorksFor = values.GetSchemas<IOrganization>("worksFor"),
                Address = new Values<IPostalAddress, string>(values.GetSchemas<IPostalAddress>("address"))
            };
        }
    }
}
