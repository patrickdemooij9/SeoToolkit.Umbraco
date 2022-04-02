using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Interfaces;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class DisplayCollection : BuilderCollectionBase<IDisplayProvider>
    {
        public DisplayCollection(Func<IEnumerable<IDisplayProvider>> items) : base(items)
        {
        }
    }
}
