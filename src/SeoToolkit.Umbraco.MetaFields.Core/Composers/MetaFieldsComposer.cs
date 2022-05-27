using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Services.SettingsService;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using SeoToolkit.Umbraco.MetaFields.Core.Common.DisplayProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups;
using SeoToolkit.Umbraco.MetaFields.Core.Components;
using SeoToolkit.Umbraco.MetaFields.Core.Config;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Mappers;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Providers;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.MetaFields.Core.Services.MetaFieldsService;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;

namespace SeoToolkit.Umbraco.MetaFields.Core.Composers
{
    public class MetaFieldsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:MetaFields");
            builder.Services.Configure<MetaFieldsAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<MetaFieldsConfigModel>), typeof(MetaFieldsConfigurationService));

            var disabledModules = section?.Get<MetaFieldsAppSettingsModel>()?.DisabledModules ?? Array.Empty<string>();

            if (disabledModules.Contains(DisabledModuleConstant.All))
            {
                builder.Components().Append<DisableModuleComponent>();
                return;
            }

            builder.Components().Append<EnableModuleComponent>();

            builder.Services.AddTransient(typeof(IRepository<DocumentTypeSettingsDto>), typeof(MetaFieldsSettingsDatabaseRepository));
            builder.Services.AddTransient(typeof(IMetaFieldsSettingsService), typeof(MetaFieldsSettingsService));
            builder.Services.AddTransient(typeof(IMetaFieldsService), typeof(MetaFieldsService));
            builder.Services.AddTransient(typeof(IMetaTagsProvider), typeof(DefaultMetaTagsProvider));
            builder.Services.AddTransient(typeof(IMetaFieldsValueService), typeof(MetaFieldsValueService));
            builder.Services.AddTransient(typeof(IMetaFieldsValueRepository), typeof(MetaFieldsDatabaseRepository));
            builder.Services.AddTransient(typeof(IMetaFieldsSettingsService), typeof(MetaFieldsSettingsService));

            builder.WithCollectionBuilder<SeoFieldCollectionBuilder>()
                .Add<SeoTitleField>()
                .Add<SeoDescriptionField>()
                .Add<OpenGraphTitleField>()
                .Add<OpenGraphDescriptionField>()
                .Add<OpenGraphImageField>()
                .Add<CanonicalUrlField>();

            builder.WithCollectionBuilder<SeoConverterCollectionBuilder>()
                .Add<TextSeoValueConverter>()
                .Add<PublishedContentSeoValueConverter>()
                .Add<FieldSeoValueConverter>()
                .Add<MultiplePublishedContentSeoValueConverter>();

            builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<DocumentTypeSettingsMapper>();

            builder.WithCollectionBuilder<FieldProviderCollectionBuilder>()
                .Add<InheritedValueFieldProvider>()
                .Add<PageNameFieldProvider>();

            if (!disabledModules.Contains(DisabledModuleConstant.DocumentTypeContextApp))
            {
                builder.WithCollectionBuilder<DisplayCollectionBuilder>()
                    .Add<MetaFieldsDocumentSettingsDisplayProvider>();

                builder.WithCollectionBuilder<SeoDisplayCollectionBuilder>()
                    .Add<MetaFieldsContentDisplayProvider>();

                builder.WithCollectionBuilder<SeoGroupCollectionBuilder>()
                    .Append<MetaFieldsGroup>()
                    .Append<OpenGraphFieldsGroup>()
                    .Append<OthersFieldGroup>();
            }
        }
    }
}
