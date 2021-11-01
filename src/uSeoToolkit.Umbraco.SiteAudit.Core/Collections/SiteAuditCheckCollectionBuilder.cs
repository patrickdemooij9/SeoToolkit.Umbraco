using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    public class SiteAuditCheckCollectionBuilder : OrderedCollectionBuilderBase<SiteAuditCheckCollectionBuilder, SiteAuditCheckCollection, ISiteCheck>
    {
        protected override SiteAuditCheckCollectionBuilder This => this;
    }
}
