namespace SeoToolkit.Umbraco.Common.Core.ContentApps
{
    /*public class SeoContentAppFactory
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SeoContentAppFactory(ISeoSettingsService seoSettingsService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _seoSettingsService = seoSettingsService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            if (source is not IContent content || !_seoSettingsService.IsEnabled(content.ContentTypeId))
                return null;

            using var scope = _serviceScopeFactory.CreateScope();

            var displayProviders = scope.ServiceProvider.GetService<SeoDisplayCollection>();
            if (displayProviders is null || displayProviders.Count == 0) return null;

            return new ContentApp
            {
                Name = "SEO",
                Alias = "seoContent",
                Icon = "icon-globe-alt",
                Weight = 100,
                View = "/App_Plugins/SeoToolkit/ContentApps/Content/seoContent.html"
            };
        }
    }*/
}
