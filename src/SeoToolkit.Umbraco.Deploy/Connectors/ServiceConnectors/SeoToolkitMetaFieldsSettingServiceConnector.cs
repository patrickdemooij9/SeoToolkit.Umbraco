using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SeoToolkit.Umbraco.Deploy.Artifacts;
using SeoToolkit.Umbraco.Deploy.Configuration;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
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
            => Task.FromResult(metaFieldsSettingsService.Get(id));

        public override async IAsyncEnumerable<DocumentTypeSettingsDto> GetEntitiesAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var entity in metaFieldsSettingsService.GetAll())
            {
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
            foreach (var (seoField, valueDto) in entity.Fields)
            {
                var json = valueDto.Value is null ? null : JsonConvert.SerializeObject(valueDto.Value);
                foreach (var referencedUdi in UdiJsonHelper.FindUdis(json))
                {
                    dependencies.Add(new SeoToolkitArtifactDependency(referencedUdi));
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
            if (contentType is null)
            {
                return Task.CompletedTask;
            }

            // Convergent restore rebuilds the DTO from the artifact so removed fields and
            // inheritance are dropped; overwrite-only merges into the existing target DTO.
            var dto = PruneMissing
                ? new DocumentTypeSettingsDto { Content = contentType }
                : state.Entity ?? new DocumentTypeSettingsDto { Content = contentType };
            dto.Content = contentType;

            if (state.Artifact.InheritanceUdi is not null)
            {
                dto.Inheritance = contentTypeService.Get(state.Artifact.InheritanceUdi.Guid);
            }
            else if (PruneMissing)
            {
                dto.Inheritance = null;
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
                    valueDto.Value = seoField.Editor.ValueConverter.ConvertDatabaseToObject(
                        JsonConvert.DeserializeObject(field.Value));
                }

                if (!dto.Fields.TryAdd(seoField, valueDto))
                {
                    dto.Fields[seoField] = valueDto;
                }
            }

            metaFieldsSettingsService.Set(dto);
            return Task.CompletedTask;
        }
    }
}
