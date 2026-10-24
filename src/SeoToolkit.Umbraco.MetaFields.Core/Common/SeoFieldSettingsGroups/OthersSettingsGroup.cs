using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class OthersSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.Others;
    public string Name => "Other";
    public string Description => "Fields that don't belong to any of the other groups.";
    public string Icon => "icon-settings";
}
