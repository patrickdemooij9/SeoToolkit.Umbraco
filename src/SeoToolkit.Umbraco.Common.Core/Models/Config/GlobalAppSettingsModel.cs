namespace SeoToolkit.Umbraco.Common.Core.Models.Config
{
    public class GlobalAppSettingsModel
    {
        public bool AutomaticSitemapsInRobotsTxt { get; set; } = true;
        public bool EnableSeoSettingsByDefaultForTemplated { get; set; } = true;
        public bool SupressContentAppSavingNotification { get; set; }
    }
}
