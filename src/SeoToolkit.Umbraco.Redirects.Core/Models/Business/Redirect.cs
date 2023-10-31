using System;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;
using SeoToolkit.Umbraco.Redirects.Core.Extensions;
using Umbraco.Cms.Core.Models.Membership;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.Business
{
    public class Redirect
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRegex { get; set; }
        public Domain Domain { get; set; }
        public string CustomDomain { get; set; }
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }
        public IPublishedContent NewNode { get; set; }
        public ILanguage NewNodeCulture { get; set; }
        public DateTime LastUpdated { get; set; }
        public int CreatedBy { get; set; }
        public int RedirectCode { get; set; }

        public string GetNewUrl()
        {
            return NewUrl.IfNullOrWhiteSpace(NewNode?.Url(NewNodeCulture?.IsoCode?.ToLowerInvariant()));
        }
    }
}
