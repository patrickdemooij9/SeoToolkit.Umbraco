using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldPreviewers
{
    public class SocialMediaPreviewer : ISeoFieldPreviewer
    {
        public string Group => SeoFieldGroupConstants.SocialMediaGroup;
        public string View => "/App_Plugins/SeoToolkit/MetaFields/Interface/Previewers/OpenGraph/openGraphPreviewer.html";
    }
}
