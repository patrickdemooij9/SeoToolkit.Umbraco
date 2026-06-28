using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using System;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels
{
    public class RedirectViewModel
    {
        public int Id { get; set; }

        public Guid Key { get; set; }

        public int? Domain { get; set; }

        public string CustomDomain { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsRegex { get; set; }

        public string OldUrl { get; set; }

        public string NewUrl { get; set; }

        public Guid? NewNodeId { get; set; }

        public string NewCultureIso { get; set; }

        public int RedirectCode { get; set; }

        public string LastUpdated { get; set; }

        public RedirectViewModel(Redirect redirect, string isoCode)
        {
            Id = redirect.Id;
            Key = redirect.Key;
            Domain = redirect.Domain?.Id;
            CustomDomain = redirect.CustomDomain;
            IsEnabled = redirect.IsEnabled;
            IsRegex = redirect.IsRegex;
            OldUrl = redirect.OldUrl.IfNullOrWhiteSpace("/");
            NewUrl = redirect.NewUrl;
            NewNodeId = redirect.NewNode?.Key;
            NewCultureIso = isoCode;
            RedirectCode = redirect.RedirectCode;
            LastUpdated = redirect.LastUpdated.ToString("G");
        }
    }
}
