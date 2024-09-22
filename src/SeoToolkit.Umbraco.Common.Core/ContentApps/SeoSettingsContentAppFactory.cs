using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.Common.Core.Collections;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace SeoToolkit.Umbraco.Common.Core.ContentApps
{
    /*public class SeoSettingsContentAppFactory : IContentAppFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SeoSettingsContentAppFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is not IContentType)
                return null;

            using var scope = _serviceScopeFactory.CreateScope();

            var displayProviders = scope.ServiceProvider.GetService<DisplayCollection>();
            if (displayProviders is null || displayProviders.Count == 0) return null;

            return new ContentApp
            {
                Name = "SEO",
                Alias = "seoSettings",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/SeoToolkit/ContentApps/DocumentType/seoSettings.html"
            };
        }
    }*/
}
