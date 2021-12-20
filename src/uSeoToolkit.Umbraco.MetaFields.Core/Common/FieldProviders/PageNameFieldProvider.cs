using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
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
