using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces
{
    public interface IMetaTagsProvider
    {
        event EventHandler<MetaTagsModel> BeforeMetaTagsGet;

        MetaTagsModel Get(IPublishedContent content);
    }
}
