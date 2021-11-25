using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoConverterCollectionBuilder : WeightedCollectionBuilderBase<SeoConverterCollectionBuilder, SeoConverterCollection, ISeoValueConverter>
    {
        protected override SeoConverterCollectionBuilder This => this;
    }
}
