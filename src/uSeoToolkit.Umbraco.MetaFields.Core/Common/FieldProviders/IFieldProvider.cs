using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders
{
    public interface IFieldProvider
    {
        IEnumerable<FieldItemViewModel> GetFieldItems();
        object HandleFieldItem(FieldsItem fieldsItem, IPublishedContent content, string fieldAlias);
    }
}
