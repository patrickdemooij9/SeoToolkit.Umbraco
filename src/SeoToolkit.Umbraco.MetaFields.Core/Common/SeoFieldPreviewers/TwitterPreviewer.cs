using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers
{
    public class TwitterPreviewer : ISeoFieldPreviewer
    {
        public string Group => SeoFieldGroupConstants.TwitterGroup;
        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/OpenGraph/openGraphPreviewer.html";
    }
}
