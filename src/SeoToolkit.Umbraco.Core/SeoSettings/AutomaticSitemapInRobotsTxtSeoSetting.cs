using SeoToolkit.Umbraco.Common.Core.Interfaces;
using System;

namespace SeoToolkit.Umbraco.Core.SeoSettings
{
    public class AutomaticSitemapInRobotsTxtSeoSetting : ISeoKeyValueSetting
    {
        public const string SettingKey = "automaticSitemapInRobotsTxt";

        public string Key => SettingKey;

        public string Title => "Automatically link sitemaps to robots.txt";

        public string Description => "When this is enabled, it will automatically add sitemap entries to your robots.txt for indexing.";

        public string PropertyAlias => "Umb.PropertyEditorUi.Toggle";

        public Type EditorType => typeof(bool);
    }
}
