using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, UdiType.GuidUdi)]
    public class SeoToolkitMetaFieldsSettingServiceConnector(
        IMetaFieldsSettingsService metaFieldsSettingsService,
        IContentTypeService contentTypeService,
        SeoFieldCollection seoFieldCollection,
        IOptionsMonitor<SeoToolkitDeploySettings> settings)
        : SeoToolkitEntityServiceConnectorBase<MetaFieldsSettingArtifact, DocumentTypeSettingsDto>(settings)
    {
        public override string UdiEntityType => SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting;

        protected override string OpenUdiName => "All SeoToolkit meta field settings";

        protected override int[] ProcessPasses => [2];

        public override string GetEntityName(DocumentTypeSettingsDto entity)
            => entity.Content.Name ?? entity.Content.Alias;

        protected override GuidUdi GetEntityUdi(DocumentTypeSettingsDto entity)
            => new(UdiEntityType, entity.Content.Key);

        public override Task<DocumentTypeSettingsDto?> GetEntityAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = metaFieldsSettingsService.Get(id);
            // A settings row whose content type was deleted maps to a null Content; skip it rather
            // than let GetEntityName/GetEntityUdi dereference null and fail the whole export.
            return Task.FromResult(entity?.Content is null ? null : entity);
        }

        public override async IAsyncEnumerable<DocumentTypeSettingsDto> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in metaFieldsSettingsService.GetAll())
            {
                // Skip orphaned rows (content type deleted) — their Content is null.
                if (entity?.Content is null)
                {
                    continue;
                }
                yield return entity;
            }
        }

        public override Task<MetaFieldsSettingArtifact?> GetArtifactAsync(
            GuidUdi? udi, DocumentTypeSettingsDto? entity, CancellationToken cancellationToken = default)
        {
            if (udi is null || entity is null)
            {
                return Task.FromResult<MetaFieldsSettingArtifact?>(null);
            }

            var dependencies = new ArtifactDependencyCollection
            {
                new SeoToolkitArtifactDependency(new GuidUdi(Constants.UdiEntityType.DocumentType, entity.Content.Key)),
            };

            GuidUdi? inheritanceUdi = null;
            if (entity.Inheritance is not null)
            {
                inheritanceUdi = new GuidUdi(UdiEntityType, entity.Inheritance.Key);
                dependencies.Add(new SeoToolkitArtifactDependency(inheritanceUdi));
            }

            var fields = new List<MetaFieldsSettingField>();
            // Order by alias so the serialized artifact (and therefore its checksum) is stable
            // regardless of the order the fields come back from storage.
            foreach (var (seoField, valueDto) in entity.Fields.OrderBy(f => f.Key.Alias, StringComparer.Ordinal))
            {
                var converter = seoField.Editor.ValueConverter;

                // Round-trip through the portable editor wire format rather than serializing the
                // object form directly: the object form of a media field is an IPublishedContent,
                // which either throws (self-referencing loop) or emits an environment-specific blob.
                var editorValue = valueDto.Value is null ? null : converter.ConvertObjectToEditorValue(valueDto.Value);
                var json = editorValue is null ? null : JsonConvert.SerializeObject(editorValue);

                // UDIs embedded in the value JSON (e.g. RTE-like content).
                foreach (var referencedUdi in UdiJsonHelper.FindUdis(json))
                {
                    dependencies.Add(new SeoToolkitArtifactDependency(referencedUdi));
                }

                // GUID-based media references (a bare media key, not a umb://media/... string).
                if (valueDto.Value is not null && converter is IMediaReferenceConverter mediaConverter)
                {
                    foreach (var mediaKey in mediaConverter.GetReferencedMediaKeys(valueDto.Value))
                    {
                        dependencies.Add(new SeoToolkitArtifactDependency(
                            new GuidUdi(Constants.UdiEntityType.Media, mediaKey)));
                    }
                }

                fields.Add(new MetaFieldsSettingField
                {
                    Alias = seoField.Alias,
                    UseInheritedValue = valueDto.UseInheritedValue,
                    Value = json,
                });
            }

            return Task.FromResult<MetaFieldsSettingArtifact?>(new MetaFieldsSettingArtifact(udi, dependencies)
            {
                Name = GetEntityName(entity),
                InheritanceUdi = inheritanceUdi,
                Fields = fields,
            });
        }

        public override Task ProcessAsync(
            ArtifactDeployState<MetaFieldsSettingArtifact, DocumentTypeSettingsDto> state,
            IDeployContext context,
            int pass,
            CancellationToken cancellationToken = default)
        {
            state.NextPass = GetNextPass(pass);

            if (pass != 2 || IsDisabled)
            {
                return Task.CompletedTask;
            }

            var contentType = contentTypeService.Get(state.Artifact.Udi.Guid);
            if (contentType is null || contentType.IsElement)
            {
                // Target has no such content type, or it's an element type — MetaFields settings
                // can't be stored on elements and Set would throw ArgumentException, aborting the
                // whole deploy. Skip this entity instead.
                return Task.CompletedTask;
            }

            // Build a fresh DTO rather than mutating state.Entity: that instance is the service's
            // 30-minute cached DocumentTypeSettingsDto, so mutating it in place would corrupt the
            // runtime cache for every other reader. The artifact is authoritative: a fresh DTO
            // replaces the target exactly, dropping target-only fields/inheritance so it converges.
            var dto = new DocumentTypeSettingsDto { Content = contentType };

            if (state.Artifact.InheritanceUdi is not null)
            {
                dto.Inheritance = contentTypeService.Get(state.Artifact.InheritanceUdi.Guid);
            }

            foreach (var field in state.Artifact.Fields)
            {
                var seoField = seoFieldCollection.Get(field.Alias);
                if (seoField is null)
                {
                    continue; // field type not installed in target; skip
                }

                var valueDto = new DocumentTypeValueDto { UseInheritedValue = field.UseInheritedValue };
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    // The artifact holds the editor wire format; convert it to the database form
                    // the same way an editor save does, so Set persists a portable value.
                    valueDto.Value = seoField.Editor.ValueConverter.ConvertEditorToDatabaseValue(
                        JsonConvert.DeserializeObject(field.Value));
                }

                dto.Fields[seoField] = valueDto;
            }

            metaFieldsSettingsService.Set(dto);
            return Task.CompletedTask;
        }
    }
}
