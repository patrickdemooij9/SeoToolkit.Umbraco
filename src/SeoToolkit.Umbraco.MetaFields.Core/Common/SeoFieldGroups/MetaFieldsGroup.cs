using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups
{
    public class MetaFieldsGroup : ISeoFieldGroup
    {
        public string Alias => SeoFieldGroupConstants.MetaFieldsGroup;
        public string Name => "Meta Fields";
        public string Description => "Fields that indicate how your page will look within the Google search results";
        public ISeoFieldPreviewer Previewer { get; }
    }
}
