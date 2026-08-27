#nullable enable
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    /// <summary>
    /// Registration point for checks. Ordered so a package can control where its checks appear
    /// in the catalogue relative to the built-in ones.
    /// </summary>
    public class SeoCheckCollectionBuilder
        : OrderedCollectionBuilderBase<SeoCheckCollectionBuilder, SeoCheckCollection, ISeoCheck>
    {
        protected override SeoCheckCollectionBuilder This => this;
    }
}
