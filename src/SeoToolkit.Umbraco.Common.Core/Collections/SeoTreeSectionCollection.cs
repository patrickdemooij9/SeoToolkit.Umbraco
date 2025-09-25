using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoTreeSectionCollection : BuilderCollectionBase<ISeoTreeSection>
    {
        public SeoTreeSectionCollection(Func<IEnumerable<ISeoTreeSection>> items) : base(items)
        {
        }
    }
}
