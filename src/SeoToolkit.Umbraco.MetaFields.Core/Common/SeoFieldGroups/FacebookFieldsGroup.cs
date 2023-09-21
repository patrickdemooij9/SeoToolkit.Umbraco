using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups
{
    public class FacebookFieldsGroup : ISeoFieldGroup
    {
        public string Alias => SeoFieldGroupConstants.FacebookGroup;
        public string Name => "Facebook";
        public string Description => "Fields that indicate how your page will look on Facebook";
        public ISeoFieldPreviewer Previewer { get; }
    }
}
