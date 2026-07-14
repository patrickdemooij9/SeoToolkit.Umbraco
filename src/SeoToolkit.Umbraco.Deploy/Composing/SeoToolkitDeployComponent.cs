using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Infrastructure.Disk;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComponent(IDiskEntityService diskEntityService) : IAsyncComponent
    {
        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            RegisterUdiTypes();
            InitializeDiskRefreshers();
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

        private void InitializeDiskRefreshers()
        {
            // Settings-like entity types are written to disk as .uda artifacts. Per-node types
            // (MetaFieldsValue / SitemapContent) are content-like data and are deliberately NOT
            // disk-registered — they ride along with content transfers instead.
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.Script);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.KeyValues);
        }
    }
}
