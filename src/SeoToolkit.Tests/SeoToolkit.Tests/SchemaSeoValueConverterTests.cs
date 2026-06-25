using Moq;
using Schema.NET;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.EditorConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SchemaSeoValueConverterTests
    {
        private SchemaResolverCollection _resolvers = null!;

        [SetUp]
        public void SetUp()
        {
            var mediaConverter = new UmbracoMediaConverter(Mock.Of<IUmbracoContextFactory>());
            _resolvers = new SchemaResolverCollection(() => new ISchemaResolver[]
            {
                new OrganizationSchemaResolver(mediaConverter),
                new PostalAddressSchemaResolver()
            });
        }

        [Test]
        public void Convert_DoesNotLeakAutoRenderDocTypeSchemasIntoNestedProperties()
        {
            var docTypeKey = Guid.NewGuid();
            var organizationId = Guid.NewGuid();
            var referencedAddressId = Guid.NewGuid();
            var autoRenderAddressId = Guid.NewGuid();

            // A postal address the Company's Organization explicitly references on its nested "address" property.
            var referencedAddress = new SchemaEntryDto
            {
                Id = referencedAddressId,
                SchemaAlias = "postalAddress",
                OwnerType = "schemaEntry",
                Properties = new()
                {
                    ["streetAddress"] = new SchemaPropertyValue { Value = "123 Referenced Street" }
                }
            };

            // A document-type postal address set to auto-render. It must render at the top level,
            // but must NOT leak into the Organization's nested address array.
            var autoRenderAddress = new SchemaEntryDto
            {
                Id = autoRenderAddressId,
                SchemaAlias = "postalAddress",
                OwnerType = "documentType",
                OwnerKey = docTypeKey,
                RenderAutomatically = true,
                Properties = new()
                {
                    ["streetAddress"] = new SchemaPropertyValue { Value = "999 Auto-Render Avenue" }
                }
            };

            // The content-level Organization on the Company, whose nested address references referencedAddress.
            var organization = new SchemaEntryDto
            {
                Id = organizationId,
                SchemaAlias = "organization",
                OwnerType = "schemaEntry",
                Properties = new()
                {
                    ["name"] = new SchemaPropertyValue { Value = "Test Company" },
                    ["address"] = new SchemaPropertyValue { Value = new[] { referencedAddressId } }
                }
            };

            var entriesById = new Dictionary<Guid, SchemaEntryDto>
            {
                [organizationId] = organization,
                [referencedAddressId] = referencedAddress,
                [autoRenderAddressId] = autoRenderAddress
            };

            var schemaEntryService = new Mock<ISchemaEntryService>();
            schemaEntryService.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
                .Returns((IEnumerable<Guid> ids) => ids
                    .Where(entriesById.ContainsKey)
                    .Select(id => entriesById[id])
                    .ToList());
            schemaEntryService.Setup(x => x.GetAll("documentType", docTypeKey))
                .Returns(new[] { autoRenderAddress });

            var contentType = new Mock<IPublishedContentType>();
            contentType.Setup(x => x.Key).Returns(docTypeKey);
            var content = new Mock<IPublishedContent>();
            content.Setup(x => x.ContentType).Returns(contentType.Object);

            var converter = new SchemaSeoValueConverter(_resolvers, schemaEntryService.Object);

            var result = (IThing[])converter.Convert(new[] { organizationId }, content.Object, "schema");

            // The Organization's nested address array must contain ONLY the referenced address,
            // not the auto-rendering document-type address.
            var organizationSchema = result.OfType<Organization>().Single();
            var organizationJson = organizationSchema.ToString();
            Assert.That(organizationJson, Does.Contain("123 Referenced Street"));
            Assert.That(organizationJson, Does.Not.Contain("999 Auto-Render Avenue"));

            // Auto-render still works: the document-type address renders as its own top-level schema.
            var topLevelAddresses = result.OfType<PostalAddress>().ToList();
            Assert.That(topLevelAddresses, Has.Count.EqualTo(1));
            Assert.That(topLevelAddresses[0].ToString(), Does.Contain("999 Auto-Render Avenue"));
        }
    }
}
