using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Common.Core.Interfaces;

namespace uSeoToolkit.Umbraco.Common.Core.Collections
{
    public class DisplayCollection : BuilderCollectionBase<IDisplayProvider>
    {
        public DisplayCollection(Func<IEnumerable<IDisplayProvider>> items) : base(items)
        {
        }
    }
}
