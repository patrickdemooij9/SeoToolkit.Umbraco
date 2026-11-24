using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class TwitterSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.Twitter;
    public string Name => "X (Twitter)";
    public string Description => "Card settings for links shared on X.";
    public string Icon => "icon-chat";
}
