using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace uSeoToolkit.Umbraco.Redirects.Core.Models.Business
{
    public class Redirect
    {
        public int Id { get; set; }
        public bool IsRegex { get; set; }
        public Domain Domain { get; set; }
        public string CustomDomain { get; set; }
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }
        public IPublishedContent NewNode { get; set; }
        public DateTime LastUpdated { get; set; }
        public int RedirectCode { get; set; }
        public string Notes { get; set; }

        public string GetNewUrl()
        {
            return NewUrl.IfNullOrWhiteSpace(NewNode?.Url());
        }
    }
}
