using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoFieldCollectionBuilder : WeightedCollectionBuilderBase<SeoFieldCollectionBuilder, SeoFieldCollection, ISeoField>
    {
        private readonly List<Action<ISeoField>> _updates = new();

        protected override SeoFieldCollectionBuilder This => this;

        /// <summary>
        /// Registers a change that is applied to every field of type <typeparamref name="TField"/> once the collection is created.
        /// </summary>
        public SeoFieldCollectionBuilder Update<TField>(Action<TField> update)
            where TField : ISeoField
        {
            ArgumentNullException.ThrowIfNull(update);

            _updates.Add(field =>
            {
                if (field is TField typedField)
                    update(typedField);
            });
            return This;
        }

        protected override IEnumerable<ISeoField> CreateItems(IServiceProvider factory)
        {
            var items = new List<ISeoField>(base.CreateItems(factory));
            foreach (var update in _updates)
            {
                foreach (var item in items)
                {
                    update(item);
                }
            }
            return items;
        }
    }
}
