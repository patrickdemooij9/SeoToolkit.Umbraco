using SeoToolkit.Umbraco.Sitemap.Core.Interfaces;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Sitemap.Core.Collections
{
    public class SitemapCollectionProviderCollection : BuilderCollectionBase<ISitemapCollectionProvider>
    {
        public SitemapCollectionProviderCollection(Func<IEnumerable<ISitemapCollectionProvider>> items) : base(items)
        {
        }
    }
}
