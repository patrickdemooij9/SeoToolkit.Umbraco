using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using uSeoToolkit.Umbraco.Common.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Collections;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using uSeoToolkit.Umbraco.MetaFields.Core.Components;
using uSeoToolkit.Umbraco.MetaFields.Core.ContentApps;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using uSeoToolkit.Umbraco.MetaFields.Core.Mappers;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using uSeoToolkit.Umbraco.MetaFields.Core.Providers;
using uSeoToolkit.Umbraco.MetaFields.Core.Repositories.DocumentTypeSettingsRepository;
using uSeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.SeoService;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.SeoValueService;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Composers
{
    public class MetaFieldsComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<MetaFieldsDatabaseComponent>();
            builder.Components().Append<EnableModuleComponent>();

            builder.ContentApps().Append<MetaFieldsDocumentSettingsContentAppFactory>();
            builder.ContentApps().Append<MetaFieldsSeoSettingsAppFactory>();

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
                .Add<FieldSeoValueConverter>();

            builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<DocumentTypeSettingsMapper>();

            builder.WithCollectionBuilder<FieldProviderCollectionBuilder>()
                .Add<InheritedValueFieldProvider>()
                .Add<PageNameFieldProvider>();
        }
    }
}
