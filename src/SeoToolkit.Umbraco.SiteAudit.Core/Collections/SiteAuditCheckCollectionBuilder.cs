using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    public class SiteAuditCheckCollectionBuilder : OrderedCollectionBuilderBase<SiteAuditCheckCollectionBuilder, SiteAuditCheckCollection, ISiteCheck>
    {
        protected override SiteAuditCheckCollectionBuilder This => this;
    }
}
