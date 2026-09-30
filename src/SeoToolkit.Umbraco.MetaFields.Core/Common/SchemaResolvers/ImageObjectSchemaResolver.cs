using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class ImageObjectSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        public ImageObjectSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Image Object";
        public string Alias => "imageObject";

        public SchemaProperty[] Properties =>
            [
                new("contentUrl", "Image", "Umb.PropertyEditorUi.MediaPicker", valueConverter: _mediaConverter),
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("caption", "Caption", "Umb.PropertyEditorUi.TextBox"),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("creditText", "Credit Text", "Umb.PropertyEditorUi.TextBox"),
                new("copyrightNotice", "Copyright Notice", "Umb.PropertyEditorUi.TextBox"),
                new("license", "License Url", "Umb.PropertyEditorUi.TextBox"),
                new("acquireLicensePage", "Acquire License Page Url", "Umb.PropertyEditorUi.TextBox"),
                SchemaValueHelper.NestedSchemaProperty("creator", "Creator", "person", "organization")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var contentUrl = values.GetUri("contentUrl");
            if (contentUrl is null)
                return null;

            var caption = values.GetString("caption");
            var license = values.GetUri("license");

            return new ExtendedImageObject
            {
                ContentUrl = contentUrl,
                Name = values.GetString("name"),
                Caption = caption is null ? default : new Values<IMediaObject, string>(caption),
                Description = values.GetString("description"),
                CreditText = values.GetString("creditText"),
                CopyrightNotice = values.GetString("copyrightNotice"),
                License = license is null ? default : new Values<ICreativeWork, Uri>(license),
                AcquireLicensePage = values.GetUri("acquireLicensePage"),
                Creator = values.GetOrganizationsOrPersons("creator")
            };
        }
    }
}
