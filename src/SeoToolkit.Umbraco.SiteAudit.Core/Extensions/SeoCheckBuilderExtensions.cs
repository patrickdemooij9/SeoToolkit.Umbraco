#nullable enable
using Umbraco.Cms.Core.DependencyInjection;
using SeoToolkit.Umbraco.SiteAudit.Core.Collections;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Extensions
{
    public static class SeoCheckBuilderExtensions
    {
        /// <summary>
        /// Adds checks to the site audit. This is the whole integration surface for an add-on
        /// package - a composer, one call, and the checks show up in the catalogue with their
        /// options, categories and scoring already wired up:
        /// <code>
        /// public void Compose(IUmbracoBuilder builder)
        /// {
        ///     builder.SeoChecks()
        ///         .Append&lt;MyTitleCheck&gt;()
        ///         .Append&lt;MyDuplicateContentCheck&gt;();
        /// }
        /// </code>
        /// </summary>
        public static SeoCheckCollectionBuilder SeoChecks(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<SeoCheckCollectionBuilder>();
    }
}
