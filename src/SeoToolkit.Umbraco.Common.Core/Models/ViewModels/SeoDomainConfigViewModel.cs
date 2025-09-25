using Umbraco.Cms.Core.Models;

namespace SeoToolkit.Umbraco.Common.Core.Models.ViewModels
{
    public class SeoDomainConfigViewModel
    {
        public IDomain[] Domains { get; set; }
        public SeoDomainModuleSettingViewModel[] ModuleSettings { get; set; }

        public SeoDomainConfigViewModel()
        {
            Domains = [];
            ModuleSettings = [];
        }
    }
}
