using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;

namespace SeoToolkit.Umbraco.MetaFields.Core.Services.SeoService
{
    public class MetaFieldsService : IMetaFieldsService
    {
        private readonly IMetaTagsProvider _metaTagsProvider;

        public MetaFieldsService(IMetaTagsProvider metaTagsProvider)
        {
            _metaTagsProvider = metaTagsProvider;
        }

        public MetaTagsModel Get(IPublishedContent content)
        {
            return _metaTagsProvider.Get(content);
        }
    }
}
