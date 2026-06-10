using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Controllers;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SchemaResolvers;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEditor;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.PostModels;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SchemaEntry.ViewModels;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SchemaEntryService;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    [ApiExplorerSettings(GroupName = "Backoffice SeoToolkit MetaFields")]
    [BackOfficeRoute("seoToolkit/schema")]
    public class SchemaController : SeoToolkitAuthenticatedControllerBase
    {
        private readonly SchemaResolverCollection _schemaResolvers;
        private readonly ISchemaEntryService _schemaEntryService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public SchemaController(SchemaResolverCollection schemaResolvers, ISchemaEntryService schemaEntryService, IUmbracoContextFactory umbracoContextFactory)
        {
            _schemaResolvers = schemaResolvers;
            _schemaEntryService = schemaEntryService;
            _umbracoContextFactory = umbracoContextFactory;
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
                    PropertyEditor = prop.PropertyEditor,
                    AllowReference = prop.AllowReference,
                    Config = prop.Config
                }).ToArray()
            }).ToArray();

            return Ok(schemas);
        }

        [HttpGet("entries")]
        [ProducesResponseType(typeof(SchemaEntryViewModel[]), 200)]
        public IActionResult GetEntries(string ownerType, Guid ownerKey)
        {
            var entries = _schemaEntryService.GetAll(ownerType, ownerKey);
            return Ok(entries.Select(MapToViewModel).ToArray());
        }

        [HttpGet("entries/{id:guid}")]
        [ProducesResponseType(typeof(SchemaEntryViewModel), 200)]
        public IActionResult GetEntry(Guid id)
        {
            var entry = _schemaEntryService.GetById(id);
            if (entry is null)
                return NotFound();
            return Ok(MapToViewModel(entry));
        }

        [HttpGet("entries/reusable")]
        [ProducesResponseType(typeof(SchemaEntryViewModel[]), 200)]
        public IActionResult GetReusableEntries(string ownerType, Guid ownerKey)
        {
            if (ownerType == "content")
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
                var content = ctx.UmbracoContext.Content.GetById(true, ownerKey);
                if (content is null)
                    return Ok(Array.Empty<SchemaEntryViewModel>());

                var docTypeEntries = _schemaEntryService.GetAll("documentType", content.ContentType.Key);
                return Ok(docTypeEntries.Select(MapToViewModel).ToArray());
            }

            var entries = _schemaEntryService.GetAll(ownerType, ownerKey);
            return Ok(entries.Select(MapToViewModel).ToArray());
        }

        [HttpPost("entries")]
        [ProducesResponseType(typeof(SchemaEntryViewModel), 200)]
        public IActionResult CreateEntry([FromBody] SchemaEntryPostModel model)
        {
            if (model is null)
                return BadRequest("Request body is required.");

            var dto = new SchemaEntryDto
            {
                Id = model.Id.HasValue && model.Id.Value != Guid.Empty ? model.Id.Value : Guid.Empty,
                OwnerType = model.OwnerType,
                OwnerKey = model.OwnerKey,
                SchemaAlias = model.SchemaAlias,
                DisplayName = model.DisplayName,
                Properties = ConvertPropertiesFromEditor(model.Properties, model.SchemaAlias)
            };

            var result = _schemaEntryService.Add(dto);
            return Ok(MapToViewModel(result));
        }

        [HttpPut("entries/{id:guid}")]
        [ProducesResponseType(typeof(SchemaEntryViewModel), 200)]
        public IActionResult UpdateEntry(Guid id, [FromBody] SchemaEntryPostModel model)
        {
            var existing = _schemaEntryService.GetById(id);
            if (existing is null)
                return NotFound();

            existing.SchemaAlias = model.SchemaAlias;
            existing.DisplayName = model.DisplayName;
            existing.Properties = ConvertPropertiesFromEditor(model.Properties, model.SchemaAlias);

            var result = _schemaEntryService.Update(existing);
            return Ok(MapToViewModel(result));
        }

        [HttpDelete("entries/{id:guid}")]
        public IActionResult DeleteEntry(Guid id)
        {
            var existing = _schemaEntryService.GetById(id);
            if (existing is null)
                return NotFound();

            _schemaEntryService.Delete(id);
            return Ok();
        }

        private SchemaEntryViewModel MapToViewModel(SchemaEntryDto dto)
        {
            var resolver = _schemaResolvers.FirstOrDefault(it =>
                it.Alias.Equals(dto.SchemaAlias, StringComparison.OrdinalIgnoreCase));

            var properties = dto.Properties?.ToDictionary(
                kv => kv.Key,
                kv =>
                {
                    var propDef = resolver?.Properties.FirstOrDefault(p => p.Alias == kv.Key);
                    var converter = propDef?.ValueConverter;
                    var editorValue = converter != null
                        ? converter.ConvertObjectToEditorValue(converter.ConvertDatabaseToObject(kv.Value.Value))
                        : kv.Value.Value;
                    return new SchemaPropertyValue
                    {
                        Value = editorValue,
                        IsReference = kv.Value.IsReference,
                        ReferenceKey = kv.Value.ReferenceKey
                    };
                }) ?? new Dictionary<string, SchemaPropertyValue>();

            return new SchemaEntryViewModel
            {
                Id = dto.Id,
                OwnerType = dto.OwnerType,
                OwnerKey = dto.OwnerKey,
                SchemaAlias = dto.SchemaAlias,
                DisplayName = dto.DisplayName,
                Properties = properties
            };
        }

        private Dictionary<string, SchemaPropertyValue> ConvertPropertiesFromEditor(
            Dictionary<string, SchemaPropertyValue> properties, string schemaAlias)
        {
            if (properties == null)
                return new Dictionary<string, SchemaPropertyValue>();

            var resolver = _schemaResolvers.FirstOrDefault(it =>
                it.Alias.Equals(schemaAlias, StringComparison.OrdinalIgnoreCase));

            return properties.ToDictionary(
                kv => kv.Key,
                kv =>
                {
                    var propDef = resolver?.Properties.FirstOrDefault(p => p.Alias == kv.Key);
                    var converter = propDef?.ValueConverter;
                    var dbValue = converter != null
                        ? converter.ConvertEditorToDatabaseValue(kv.Value.Value)
                        : kv.Value.Value;
                    return new SchemaPropertyValue
                    {
                        Value = dbValue,
                        IsReference = kv.Value.IsReference,
                        ReferenceKey = kv.Value.ReferenceKey
                    };
                });
        }
    }
}