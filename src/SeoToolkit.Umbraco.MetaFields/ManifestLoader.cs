using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;

namespace SeoToolkit.Umbraco.MetaFields
{
    internal class ManifestLoader : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<ManifestFilter>();
        }
    }

    internal class ManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "SeoToolkit.Umbraco.MetaFields",
                Version = "2.3.3",
                Scripts = new[]
                {
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/ContentApps/DocumentSettings/documentSettings.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/ContentApps/SeoSettings/seoSettings.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/MetaFields/metaFieldsPreviewer.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/OpenGraph/openGraphPreviewer.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/FieldsEditor/fieldsEditor.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/PropertyEditor/propertyEditor.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/PropertyEditor/noSelectCheckboxList.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/Components/ItemGroupPicker/itemGroupPicker.controller.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/SeoFieldEditors/seoFieldEditor.directive.js",
                    "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/previewer.directive.js"
                },
                Stylesheets = new[]
                {
                    "/App_Plugins/SeoToolkit/MetaFields/css/main.css"
                },
                BundleOptions = BundleOptions.None
            });
        }
    }
}
