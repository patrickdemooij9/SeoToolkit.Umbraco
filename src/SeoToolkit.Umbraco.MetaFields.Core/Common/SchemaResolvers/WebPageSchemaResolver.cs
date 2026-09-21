using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class WebPageSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        public WebPageSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Web Page";
        public string Alias => "webPage";

        public SchemaProperty[] Properties =>
            [
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("inLanguage", "Language", "Umb.PropertyEditorUi.TextBox"),
                new("image", "Image", "Umb.PropertyEditorUi.MediaPicker", valueConverter: _mediaConverter),
                new("datePublished", "Date Published", "Umb.PropertyEditorUi.TextBox"),
                new("dateModified", "Date Modified", "Umb.PropertyEditorUi.TextBox"),
                SchemaValueHelper.NestedSchemaProperty("isPartOf", "Is Part Of", "webSite")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var image = values.GetUri("image");
            return new WebPage
            {
                Name = values.GetString("name"),
                Url = values.GetUri("url"),
                Description = values.GetString("description"),
                InLanguage = values.GetString("inLanguage"),
                Image = image is null ? default : new Values<IImageObject, Uri>(image),
                DatePublished = values.GetDate("datePublished"),
                DateModified = values.GetDate("dateModified"),
                IsPartOf = new Values<ICreativeWork, Uri>(values.GetSchemas<ICreativeWork>("isPartOf"))
            };
        }
    }
}
