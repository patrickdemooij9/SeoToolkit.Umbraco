using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Collections
{
    public class RobotsTxtShortcutCollectionBuilder : OrderedCollectionBuilderBase<RobotsTxtShortcutCollectionBuilder, RobotsTxtShortcutCollection, IRobotsTxtShortcut>
    {
        protected override RobotsTxtShortcutCollectionBuilder This => this;
    }
}
