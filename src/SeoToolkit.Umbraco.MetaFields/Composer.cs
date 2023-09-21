using Microsoft.Extensions.Configuration;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Common.DisplayProviders;
using SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups;
using SeoToolkit.Umbraco.MetaFields.Core.Config.Models;
using System;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.MetaFields
{
    internal class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var section = builder.Config.GetSection("SeoToolkit:MetaFields");
            var settings = section?.Get<MetaFieldsAppSettingsModel>();
            var disabledModules = settings?.DisabledModules ?? Array.Empty<string>();

            if (!disabledModules.Contains(DisabledModuleConstant.DocumentTypeContextApp))
            {
                builder.WithCollectionBuilder<DisplayCollectionBuilder>()
                    .Add<MetaFieldsDocumentSettingsDisplayProvider>();

                builder.WithCollectionBuilder<SeoDisplayCollectionBuilder>()
                    .Add<MetaFieldsContentDisplayProvider>();

                builder.WithCollectionBuilder<SeoGroupCollectionBuilder>()
                    .Append<MetaFieldsGroup>()
                    .Append<OpenGraphFieldsGroup>()
                    .Append<TwitterFieldsGroup>()
                    .Append<FacebookFieldsGroup>()
                    .Append<OthersFieldGroup>();
            }
        }
    }
}
