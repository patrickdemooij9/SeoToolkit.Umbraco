using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using System.Linq;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit MetaFields")]
    [BackOfficeRoute("seoToolkit/schema")]
    public class SchemaController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SchemaResolverCollection _schemaResolvers;

        public SchemaController(SchemaResolverCollection schemaResolvers)
        {
            _schemaResolvers = schemaResolvers;
        }

        [HttpGet("types")]
        [ProducesResponseType(typeof(SchemaTypeViewModel[]), 200)]
        public IActionResult GetSchemaTypes()
        {
            var schemas = _schemaResolvers.Select(resolver => new SchemaTypeViewModel
            {
                Alias = resolver.Alias,
                Name = resolver.Name,
                Properties = resolver.Properties.Select(prop => new SchemaPropertyViewModel
                {
                    Alias = prop.Alias,
                    DisplayName = prop.DisplayName,
                    PropertyEditor = prop.PropertyEditor
                }).ToArray()
            }).ToArray();

            return Ok(schemas);
        }
    }
}