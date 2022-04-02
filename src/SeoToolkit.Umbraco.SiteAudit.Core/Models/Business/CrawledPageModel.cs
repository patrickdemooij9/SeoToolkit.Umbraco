using System;
using HtmlAgilityPack;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Business
{
    public class CrawledPageModel
    {
        public Uri Url { get; }
        public Uri[] FoundUrls { get; set; }
        public DateTime RequestStarted { get; set; }
        public DateTime RequestCompleted { get; set; }
        public int StatusCode { get; set; }
        public HtmlDocument Content { get; set; }

        public CrawledPageModel(Uri url)
        {
            Url = url;
        }
    }
}
