using Umbraco.Cms.Core.Sections;

namespace uSeoToolkit.Umbraco.Common.Core.Sections
{
    public class USeoToolkitSection : ISection
    {
        public const string SectionAlias = "uSeoToolkit";
        public const string SectionName = "uSeoToolkit";

        public string Alias => SectionAlias;

        public string Name => SectionName;
    }
}
