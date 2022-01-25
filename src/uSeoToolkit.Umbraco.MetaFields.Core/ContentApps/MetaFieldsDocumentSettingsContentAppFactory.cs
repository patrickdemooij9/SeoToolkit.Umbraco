using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace uSeoToolkit.Umbraco.MetaFields.Core.ContentApps
{
    public class MetaFieldsDocumentSettingsContentAppFactory : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            //TODO: Remove
            return null;
            if (!(source is IContentType contentType))
            {
                return null;
            }

            return new ContentApp
            {
                Name = "Seo Settings",
                Alias = "seoSettings",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/uSeoToolkitMetaFields/Interface/ContentApps/DocumentSettings/documentSettings.html"
            };
        }
    }
}
