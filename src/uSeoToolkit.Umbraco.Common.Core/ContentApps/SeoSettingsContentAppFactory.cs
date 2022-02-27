using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace uSeoToolkit.Umbraco.Common.Core.ContentApps
{
    public class SeoSettingsContentAppFactory : IContentAppFactory
    {
        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is not IContentType)
                return null;

            return new ContentApp
            {
                Name = "SEO",
                Alias = "seoSettings",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/uSeoToolkit/ContentApps/DocumentType/seoSettings.html"
            };
        }
    }
}
