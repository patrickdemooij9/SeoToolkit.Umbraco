using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Api
{
    public class MetaFieldsApiHandler : IApiDataHandler
    {
        private readonly IMetaFieldsService _metaFieldsService;

        public string Name => "metaFields";

        public MetaFieldsApiHandler(IMetaFieldsService metaFieldsService)
        {
            _metaFieldsService = metaFieldsService;
        }

        public object GetData(IPublishedContent content)
        {
            var model = _metaFieldsService.Get(content, true);
            return model.Fields.ToDictionary(it => it.Key.Alias, it => it.Value);
        }
    }
}
