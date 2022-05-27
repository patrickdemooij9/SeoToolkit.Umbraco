using System.Collections.Generic;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace SeoToolkit.Umbraco.Common.Core.ContentApps
{
    public class SeoContentAppFactory : IContentAppFactory
    {
        private readonly ISeoSettingsService _seoSettingsService;

        public SeoContentAppFactory(ISeoSettingsService seoSettingsService)
        {
            _seoSettingsService = seoSettingsService;
        }

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is not IContent { HasIdentity: true } content || !_seoSettingsService.IsEnabled(content.ContentTypeId))
                return null;

            return new ContentApp
            {
                Name = "SEO",
                Alias = "seoContent",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/SeoToolkit/ContentApps/Content/seoContent.html"
            };
        }
    }
}
