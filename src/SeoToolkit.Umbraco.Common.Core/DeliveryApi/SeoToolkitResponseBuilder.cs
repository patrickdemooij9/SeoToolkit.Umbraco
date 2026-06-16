using SeoToolkit.Umbraco.Common.Core.Collections;
using System.Collections.Generic;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SeoToolkit.Umbraco.Common.Core.DeliveryApi
{
    public class SeoToolkitResponseBuilder : ApiContentResponseBuilder
    {
        private readonly IEnumerable<IApiDataHandler> _dataHandlers;

        public SeoToolkitResponseBuilder(
            IApiContentNameProvider apiContentNameProvider,
            IApiContentRouteBuilder apiContentRouteBuilder,
            IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
            IVariationContextAccessor variationContextAccessor,
            IEnumerable<IApiDataHandler> dataHandlers) : base(apiContentNameProvider, apiContentRouteBuilder, outputExpansionStrategyAccessor, variationContextAccessor)
        {
            _dataHandlers = dataHandlers;
        }

        protected override IApiContentResponse Create(
        IPublishedContent content,
        string name,
        IApiContentRoute route,
        IDictionary<string, object?> properties)
        {
            var cultures = GetCultures(content);
            var seoToolkitData = new Dictionary<string, object>();
            foreach (var dataHandler in _dataHandlers)
            {
                var data = dataHandler.GetData(content);
                if (data != null)
                {
                    seoToolkitData[dataHandler.Name] = data;
                }
            }

            return new SeoToolkitContentResponse(
                content.Key,
                name,
                content.ContentType.Alias,
                content.CreateDate,
                content.UpdateDate,
                route,
                properties,
                cultures,
                seoToolkitData);
        }
    }
}
