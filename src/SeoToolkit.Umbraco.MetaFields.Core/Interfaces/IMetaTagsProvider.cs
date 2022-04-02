using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces
{
    public interface IMetaTagsProvider
    {
        event EventHandler<MetaTagsModel> BeforeMetaTagsGet;

        MetaTagsModel Get(IPublishedContent content);
    }
}
