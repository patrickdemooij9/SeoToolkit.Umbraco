using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Collections
{
    public class RobotsTxtShortcutCollection : BuilderCollectionBase<IRobotsTxtShortcut>
    {
        public RobotsTxtShortcutCollection(Func<IEnumerable<IRobotsTxtShortcut>> items) : base(items)
        {
        }
    }
}
