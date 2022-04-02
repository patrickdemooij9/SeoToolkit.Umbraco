using Umbraco.Cms.Core.Sections;

namespace SeoToolkit.Umbraco.Common.Core.Sections
{
    public class SeoToolkitSection : ISection
    {
        public const string SectionAlias = "SeoToolkit";
        public const string SectionName = "SeoToolkit";

        public string Alias => SectionAlias;

        public string Name => SectionName;
    }
}
