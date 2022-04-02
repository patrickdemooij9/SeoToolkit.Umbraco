using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoService;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services
{
    public interface IMetaFieldsService
    {
        MetaTagsModel Get(IPublishedContent content);
    }
}
