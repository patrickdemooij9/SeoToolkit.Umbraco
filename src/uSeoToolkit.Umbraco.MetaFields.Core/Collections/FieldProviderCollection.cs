using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.PublishedContent;
using uSeoToolkit.Umbraco.MetaFields.Core.Common.FieldProviders;
using uSeoToolkit.Umbraco.MetaFields.Core.Models.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class FieldProviderCollection : BuilderCollectionBase<IFieldProvider>
    {
        public FieldProviderCollection(Func<IEnumerable<IFieldProvider>> items) : base(items)
        {
        }

        public IEnumerable<FieldItemViewModel> GetAllItems()
        {
            var items = new List<FieldItemViewModel>();
            foreach (var provider in this)
            {
                items.AddRange(provider.GetFieldItems());
            }

            return items;
        }

        public object HandleItem(FieldsItem item, IPublishedContent content, string fieldAlias)
        {
            foreach (var provider in this)
            {
                var hasItem = provider.GetFieldItems().Any(it => it.Value == item.Value);
                if (hasItem)
                    return provider.HandleFieldItem(item, content, fieldAlias);
            }

            return null;
        }
    }
}
