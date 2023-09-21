using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups
{
    public class TwitterFieldsGroup : ISeoFieldGroup
    {
        public string Alias => SeoFieldGroupConstants.TwitterGroup;
        public string Name => "X (formerly Twitter)";
        public string Description => "Fields that indicate how your page will look on X";
        public ISeoFieldPreviewer Previewer { get; }
    }
}
