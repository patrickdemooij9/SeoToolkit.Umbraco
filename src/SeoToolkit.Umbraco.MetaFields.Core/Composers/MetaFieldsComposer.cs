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
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.MetaFieldsSettingsRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using SeoToolkit.Umbraco.MetaFields.Core.Services.MetaFieldsService;
using SeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;
using Umbraco.Cms.Core.Services;

namespace SeoToolkit.Umbraco.MetaFields.Core.Composers
{
    public class MetaFieldsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:MetaFields");
            builder.Services.Configure<MetaFieldsAppSettingsModel>(section);
            builder.Services.AddSingleton(typeof(ISettingsService<MetaFieldsConfigModel>), typeof(MetaFieldsConfigurationService));

            var settings = section?.Get<MetaFieldsAppSettingsModel>();
            var disabledModules = settings?.DisabledModules ?? Array.Empty<string>();

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
                .Add<OpenGraphUrlField>()
                .Add<CanonicalUrlField>()
                .Add<RobotsField>()
                .Add<SeoSchemaField>()
                .Add<TwitterCardTypeField>()
                .Add<TwitterSiteField>()
                .Add<TwitterCreatorField>()
                .Add<FacebookIdField>();

            if (settings?.ShowKeywordsField is true)
            {
                builder.WithCollectionBuilder<SeoFieldCollectionBuilder>().Add<KeywordsField>();
            }

            builder.WithCollectionBuilder<SeoConverterCollectionBuilder>()
                .Add<TextSeoValueConverter>()
                .Add<PublishedContentSeoValueConverter>()
                .Add<FieldSeoValueConverter>()
                .Add<MultiplePublishedContentSeoValueConverter>()
                .Add<HtmlEncodedStringSeoConverter>();

            builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<DocumentTypeSettingsMapper>();

            builder.WithCollectionBuilder<FieldProviderCollectionBuilder>()
                .Add<InheritedValueFieldProvider>()
                .Add<PageNameFieldProvider>();
        }
    }
}
