using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.SiteAudit.Core.Interfaces;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Business;

namespace SeoToolkit.Umbraco.SiteAudit.Core.SiteCrawler
{
    public class DefaultLinkParser : ILinkParser
    {
        private static string[] _ignorePrefixesList = ["mailto:", "tel:", "file:", "javascript:"];

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
                var hrefValue = link.Attributes["href"].Value.Trim();
                if (_ignorePrefixesList.Any(it => hrefValue.StartsWith(it, StringComparison.InvariantCultureIgnoreCase)))
                    continue;
                if (!Uri.TryCreate(baseUri, hrefValue, out var uriResult))
                    continue;

                yield return uriResult;
            }
        }
    }
}
