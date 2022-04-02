using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
{
    public class PageNameFieldProvider : IFieldProvider
    {
        public IEnumerable<FieldItemViewModel> GetFieldItems()
        {
            return new []
            {
                new FieldItemViewModel("Page Name", "custom-pageName") 
            };
        }

        public object HandleFieldItem(FieldsItem fieldsItem, IPublishedContent content, string fieldAlias)
        {
            return content.Name;
        }
    }
}
