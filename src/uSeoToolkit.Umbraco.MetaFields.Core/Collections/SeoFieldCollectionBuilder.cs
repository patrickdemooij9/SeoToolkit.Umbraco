using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoFieldCollectionBuilder : WeightedCollectionBuilderBase<SeoFieldCollectionBuilder, SeoFieldCollection, ISeoField>
    {
        protected override SeoFieldCollectionBuilder This => this;
    }
}
