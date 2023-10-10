using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups
{
    public class SocialMediaFieldsGroup : ISeoFieldGroup
    {
        public string Alias => SeoFieldGroupConstants.SocialMediaGroup;
        public string Name => "Social Media";
        public string Description => "Fields that indicate how your page will look on social media platforms";
        public ISeoFieldPreviewer Previewer { get; }
    }
}
