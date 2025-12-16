using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using Umbraco.Cms.Core;

namespace SeoToolkit.Umbraco.Deploy.Extensions
{
    public static class UdiExtensions
    {
        public static GuidUdi GetUdi(this RobotsTxtModel entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(SeoToolkitDeployConstants.UdiRobotsTxtEntityType, entity.Key).EnsureClosed();
        }
    }
}
