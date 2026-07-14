using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComponent : IAsyncComponent
    {
        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            RegisterUdiTypes();
            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
            => Task.CompletedTask;

        private static void RegisterUdiTypes()
        {
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.Script, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.KeyValues, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue, UdiType.GuidUdi);
            UdiParser.RegisterUdiType(SeoToolkitDeployConstants.UdiEntityType.SitemapContent, UdiType.GuidUdi);
        }
    }
}
