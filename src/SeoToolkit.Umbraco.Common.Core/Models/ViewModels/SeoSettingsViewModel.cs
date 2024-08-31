using System;

namespace SeoToolkit.Umbraco.Common.Core.Models.ViewModels
{
    public class SeoSettingsViewModel
    {
        public bool IsEnabled { get; set; }
        public bool SupressContentAppSavingNotification { get; set; }
        public SeoDisplayViewModel[] Displays { get; set; }

        public SeoSettingsViewModel()
        {
            Displays = Array.Empty<SeoDisplayViewModel>();
        }
    }
}
