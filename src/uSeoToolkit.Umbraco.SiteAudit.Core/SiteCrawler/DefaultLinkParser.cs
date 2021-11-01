using System;
using System.Collections.Generic;
using uSeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using uSeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler
{
    public class DefaultLinkParser : ILinkParser
    {
        public IEnumerable<Uri> GetLinks(CrawledPageModel page)
        {
            if (page is null)
                throw new ArgumentNullException(nameof(page));

            var links = page.Content?.DocumentNode.SelectNodes("//a[@href]");
            if (links is null)
                yield break;

            var baseUri = page.Url;
            foreach (var link in links)
            {
                var hrefValue = link.Attributes["href"].Value;
                yield return new Uri(baseUri, hrefValue);
            }
        }
    }
}
