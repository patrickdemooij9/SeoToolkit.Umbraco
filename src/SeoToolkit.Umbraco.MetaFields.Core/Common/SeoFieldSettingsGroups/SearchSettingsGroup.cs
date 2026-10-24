using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class SearchSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.Search;
    public string Name => "Search results";
    public string Description => "How pages of this type appear in Google and other search engines.";
    public string Icon => "icon-search";
}
