namespace SeoToolkit.Umbraco.Common.Core.Models.Config
{
    public class GlobalAppSettingsModel
    {
        public bool AutomaticSitemapsInRobotsTxt { get; set; } = true;
        public bool EnableSeoSettingsByDefault { get; set; } = false; // TODO: Change this in a major version to true;
        public bool SupressContentAppSavingNotification { get; set; }
        public bool EnableApiEndpoints { get; set; } = false;
    }
}
