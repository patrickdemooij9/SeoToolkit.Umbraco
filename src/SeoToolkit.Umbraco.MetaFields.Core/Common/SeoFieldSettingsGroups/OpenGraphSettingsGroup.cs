using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class OpenGraphSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.OpenGraph;
    public string Name => "Open Graph";
    public string Description => "Used when a page is shared on platforms like Facebook, LinkedIn, WhatsApp and Slack.";
    public string Icon => "icon-share-alt";
}
