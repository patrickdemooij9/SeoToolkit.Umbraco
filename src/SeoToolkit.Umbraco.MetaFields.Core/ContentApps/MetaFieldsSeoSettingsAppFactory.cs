using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;

namespace SeoToolkit.Umbraco.MetaFields.Core.ContentApps
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
            if (!(source is IContent content) || !content.HasIdentity || _documentTypeSettingsService.Get(content.ContentType.Id) is null) return null;

            return new ContentApp
            {
                Name = "SEO",
                Alias = "metaFieldsSeoSettings",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/SeoToolkit/MetaFields/Interface/ContentApps/SeoSettings/seoSettings.html"
            };
        }
    }
}
