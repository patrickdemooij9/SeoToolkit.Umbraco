using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.PostModels
{
    public class UpdateStatusCodesRedirectPostModel
    {
        public Guid[] RedirectIds { get; set; }
        public int RedirectCode { get; set; }
    }
}
