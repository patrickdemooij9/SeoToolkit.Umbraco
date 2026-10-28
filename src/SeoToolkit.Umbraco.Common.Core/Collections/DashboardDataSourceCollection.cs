using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class DashboardDataSourceCollection : BuilderCollectionBase<IDashboardDataSource>
    {
        public DashboardDataSourceCollection(Func<IEnumerable<IDashboardDataSource>> items) : base(items)
        {
        }

        public IDashboardDataSource? Get(string alias)
        {
            return this.FirstOrDefault(it => it.Alias.Equals(alias, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
