using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoDisplayCollection : BuilderCollectionBase<ISeoDisplayProvider>
    {
        public SeoDisplayCollection(Func<IEnumerable<ISeoDisplayProvider>> items) : base(items)
        {
        }
    }
}
