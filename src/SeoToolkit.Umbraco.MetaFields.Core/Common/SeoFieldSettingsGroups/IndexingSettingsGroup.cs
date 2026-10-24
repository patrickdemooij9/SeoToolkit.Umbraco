using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class IndexingSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.Indexing;
    public string Name => "Indexing";
    public string Description => "Control how search engines crawl and index pages of this type.";
    public string Icon => "icon-eye";
}
