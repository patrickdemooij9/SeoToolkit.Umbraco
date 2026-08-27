#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Collections
{
    /// <summary>
    /// Every registered check, from this package and from any add-on.
    /// </summary>
    public class SeoCheckCollection : BuilderCollectionBase<ISeoCheck>
    {
        public SeoCheckCollection(Func<IEnumerable<ISeoCheck>> items) : base(items)
        {
        }

        public IEnumerable<ISeoPageCheck> PageChecks => this.OfType<ISeoPageCheck>();

        public IEnumerable<ISeoSiteCheck> SiteChecks => this.OfType<ISeoSiteCheck>();

        public ISeoCheck? GetByAlias(string alias)
            => this.FirstOrDefault(it => string.Equals(it.Descriptor.Alias, alias, StringComparison.OrdinalIgnoreCase));
    }
}
