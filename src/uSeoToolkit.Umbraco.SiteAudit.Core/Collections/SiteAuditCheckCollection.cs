using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    public class SiteAuditCheckCollection : BuilderCollectionBase<ISiteCheck>
    {
        public SiteAuditCheckCollection(Func<IEnumerable<ISiteCheck>> items) : base(items)
        {
        }
    }
}
