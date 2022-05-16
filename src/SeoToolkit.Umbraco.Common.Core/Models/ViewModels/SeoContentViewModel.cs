using System;

namespace SeoToolkit.Umbraco.Common.Core.Models.ViewModels
{
    public class SeoContentViewModel
    {
        public SeoDisplayViewModel[] Displays { get; set; }

        public SeoContentViewModel()
        {
            Displays = Array.Empty<SeoDisplayViewModel>();
        }
    }
}
