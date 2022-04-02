using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
{
    public interface IFieldProvider
    {
        IEnumerable<FieldItemViewModel> GetFieldItems();
        object HandleFieldItem(FieldsItem fieldsItem, IPublishedContent content, string fieldAlias);
    }
}
