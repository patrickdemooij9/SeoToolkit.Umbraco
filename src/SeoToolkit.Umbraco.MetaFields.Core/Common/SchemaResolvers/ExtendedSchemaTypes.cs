using Schema.NET;
using System.Text.Json.Serialization;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers
{
    // Schema.NET does not (yet) contain every property that Google supports. These types add the
    // missing properties while still serializing with the @type of their base type.

    public class ExtendedPerson : Person
    {
        /// <summary>
        /// The job title of the person (for example, Financial Manager).
        /// </summary>
        [JsonPropertyName("jobTitle")]
        [JsonPropertyOrder(1000)]
        [JsonConverter(typeof(ValuesJsonConverter))]
        public OneOrMany<string> JobTitle { get; set; }
    }

    public class ExtendedImageObject : ImageObject
    {
        /// <summary>
        /// Text that can be used to credit person(s) and/or organization(s) associated with a published image.
        /// </summary>
        [JsonPropertyName("creditText")]
        [JsonPropertyOrder(1000)]
        [JsonConverter(typeof(ValuesJsonConverter))]
        public OneOrMany<string> CreditText { get; set; }

        /// <summary>
        /// Text of a notice appropriate for describing the copyright aspects of this image.
        /// </summary>
        [JsonPropertyName("copyrightNotice")]
        [JsonPropertyOrder(1001)]
        [JsonConverter(typeof(ValuesJsonConverter))]
        public OneOrMany<string> CopyrightNotice { get; set; }

        /// <summary>
        /// A page documenting how licenses can be purchased or otherwise acquired for this image.
        /// </summary>
        [JsonPropertyName("acquireLicensePage")]
        [JsonPropertyOrder(1002)]
        [JsonConverter(typeof(ValuesJsonConverter))]
        public OneOrMany<System.Uri> AcquireLicensePage { get; set; }
    }
}
