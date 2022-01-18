using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using uSeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace uSeoToolkit.Umbraco.MetaFields.Core.ContentApps
{
    public class MetaFieldsSeoSettingsAppFactory : IContentAppFactory
    {
        private readonly IMetaFieldsSettingsService _documentTypeSettingsService;

        public MetaFieldsSeoSettingsAppFactory(IMetaFieldsSettingsService documentTypeSettingsService)
        {
            _documentTypeSettingsService = documentTypeSettingsService;
        }

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (!(source is IContent content) || !content.HasIdentity || !_documentTypeSettingsService.IsEnabled(content)) return null;

            return new ContentApp
            {
                Name = "Seo",
                Alias = "seoSettings",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/uSeoToolkitMetaFields/Interface/ContentApps/SeoSettings/seoSettings.html"
            };
        }
    }
}
