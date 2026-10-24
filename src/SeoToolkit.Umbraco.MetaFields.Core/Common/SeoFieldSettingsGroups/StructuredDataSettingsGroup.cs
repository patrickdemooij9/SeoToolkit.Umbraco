using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldSettingsGroups;

public class StructuredDataSettingsGroup : ISeoFieldSettingsGroup
{
    public string Alias => SeoFieldSettingsGroupConstants.StructuredData;
    public string Name => "Structured data";
    public string Description => "Schema.org JSON-LD output for pages of this type.";
    public string Icon => "icon-brackets";
}
