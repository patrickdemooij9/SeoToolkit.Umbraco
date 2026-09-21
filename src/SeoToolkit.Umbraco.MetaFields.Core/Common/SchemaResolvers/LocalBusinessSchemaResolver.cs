using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    public class LocalBusinessSchemaResolver : ISchemaResolver
    {
        private readonly UmbracoMediaConverter _mediaConverter;

        public LocalBusinessSchemaResolver(UmbracoMediaConverter mediaConverter)
        {
            _mediaConverter = mediaConverter;
        }

        public string Name => "Local Business";
        public string Alias => "localBusiness";

        public SchemaProperty[] Properties =>
            [
                new("name", "Name", "Umb.PropertyEditorUi.TextBox"),
                new("url", "Url", "Umb.PropertyEditorUi.TextBox"),
                new("image", "Image", "Umb.PropertyEditorUi.MediaPicker", valueConverter: _mediaConverter),
                new("logo", "Logo", "Umb.PropertyEditorUi.MediaPicker", allowReference: false, valueConverter: _mediaConverter),
                new("description", "Description", "Umb.PropertyEditorUi.TextArea"),
                new("telephone", "Telephone", "Umb.PropertyEditorUi.TextBox"),
                new("email", "Email", "Umb.PropertyEditorUi.TextBox"),
                new("priceRange", "Price Range", "Umb.PropertyEditorUi.TextBox"),
                new("latitude", "Latitude", "Umb.PropertyEditorUi.TextBox"),
                new("longitude", "Longitude", "Umb.PropertyEditorUi.TextBox"),
                new("sameAs", "Same As (one URL per line)", "Umb.PropertyEditorUi.TextArea"),
                SchemaValueHelper.NestedSchemaProperty("address", "Address", "postalAddress"),
                SchemaValueHelper.NestedSchemaProperty("openingHoursSpecification", "Opening Hours", "openingHoursSpecification")
            ];

        public IThing ToSchema(Dictionary<string, object> values)
        {
            var image = values.GetUri("image");
            var logo = values.GetUri("logo");
            var latitude = values.GetDouble("latitude");
            var longitude = values.GetDouble("longitude");

            return new LocalBusiness
            {
                Name = values.GetString("name"),
                Url = values.GetUri("url"),
                Image = image is null ? default : new Values<IImageObject, Uri>(image),
                Logo = logo is null ? default : new Values<IImageObject, Uri>(logo),
                Description = values.GetString("description"),
                Telephone = values.GetString("telephone"),
                Email = values.GetString("email"),
                PriceRange = values.GetString("priceRange"),
                Geo = latitude is null || longitude is null
                    ? default
                    : new Values<IGeoCoordinates, IGeoShape>(new GeoCoordinates { Latitude = latitude, Longitude = longitude }),
                SameAs = values.GetUris("sameAs"),
                Address = new Values<IPostalAddress, string>(values.GetSchemas<IPostalAddress>("address")),
                OpeningHoursSpecification = values.GetSchemas<IOpeningHoursSpecification>("openingHoursSpecification")
            };
        }
    }
}
