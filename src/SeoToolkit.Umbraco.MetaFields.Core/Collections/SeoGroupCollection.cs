using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections
{
    public class SeoGroupCollection : BuilderCollectionBase<ISeoFieldGroup>
    {
        public SeoGroupCollection(Func<IEnumerable<ISeoFieldGroup>> items) : base(items)
        {
        }
    }
}
