using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels
{
    public class RedirectViewModel
    {
        public int Id { get; set; }

        public int? Domain { get; set; }

        public string CustomDomain { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsRegex { get; set; }

        public string OldUrl { get; set; }

        public string NewUrl { get; set; }

        public int? NewNodeId { get; set; }

        public int? NewCultureId { get; set; }

        public int RedirectCode { get; set; }

        public RedirectViewModel(Redirect redirect)
        {
            Id = redirect.Id;
            Domain = redirect.Domain?.Id;
            CustomDomain = redirect.CustomDomain;
            IsEnabled = redirect.IsEnabled;
            IsRegex = redirect.IsRegex;
            OldUrl = redirect.OldUrl.IfNullOrWhiteSpace("/");
            NewUrl = redirect.NewUrl;
            NewNodeId = redirect.NewNode?.Id;
            NewCultureId = redirect.NewNodeCulture?.Id;
            RedirectCode = redirect.RedirectCode;
        }
    }
}
