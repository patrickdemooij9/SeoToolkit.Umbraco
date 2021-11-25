using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services
{
    public interface ISeoService
    {
        MetaTagsModel Get(IPublishedContent content);
    }
}
