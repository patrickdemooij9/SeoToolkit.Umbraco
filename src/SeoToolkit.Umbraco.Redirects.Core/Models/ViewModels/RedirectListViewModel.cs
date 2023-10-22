using System;

namespace SeoToolkit.Umbraco.Redirects.Core.Models.ViewModels
{
    public class RedirectListViewModel
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public string Domain { get; set; }
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }
        public int StatusCode { get; set; }
        public string LastUpdated { get; set; }
    }
}
