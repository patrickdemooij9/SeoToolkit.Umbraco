using Umbraco.Cms.Core.DependencyInjection;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Collections;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Extensions
{
    public static class RobotsTxtShortcutExtensions
    {
        public static RobotsTxtShortcutCollectionBuilder RobotsTxtShortcuts(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<RobotsTxtShortcutCollectionBuilder>();
    }
}
