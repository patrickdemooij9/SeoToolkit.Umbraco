using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.MetaFieldsService
{
    public class MetaFieldsService : IMetaFieldsService
    {
        private readonly IMetaTagsProvider _metaTagsProvider;

        public MetaFieldsService(IMetaTagsProvider metaTagsProvider)
        {
            _metaTagsProvider = metaTagsProvider;
        }

        public MetaTagsModel Get(IPublishedContent content, bool includeUserValues)
        {
            return _metaTagsProvider.Get(content, includeUserValues);
        }

        public MetaTagsModel GetEmpty()
        {
            return _metaTagsProvider.GetEmpty();
        }
    }
}
