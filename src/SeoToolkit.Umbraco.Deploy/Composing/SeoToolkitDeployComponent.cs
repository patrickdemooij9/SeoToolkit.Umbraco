using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Infrastructure.Disk;
using Umbraco.Deploy.Infrastructure.Transfer;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    public class SeoToolkitDeployComponent(
        IDiskEntityService diskEntityService,
        ITransferEntityService transferEntityService) : IAsyncComponent
    {
        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            RegisterUdiTypes();
            InitializeDiskRefreshers();
            InitializeIntegratedEntities();
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
            // Settings-like entity types are written to disk as .uda artifacts and refreshed on
            // save/delete (see the disk-refresher handlers).
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SeoSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SitemapPageType);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.Script);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.DomainCollection);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.KeyValues);

            // Per-node types (MetaFieldsValue / SitemapContent) are content-like: they are NOT
            // queue-for-transfer entities and have no save/delete disk refreshers of their own.
            // They are still registered as disk entity types so their .uda travels with the node
            // when the node is exported — the content export handler attaches them as (Match)
            // dependencies, and Deploy writes/reads their artifact on a per-node basis.
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue);
            diskEntityService.RegisterDiskEntityType(SeoToolkitDeployConstants.UdiEntityType.SitemapContent);
        }

        private void InitializeIntegratedEntities()
        {
            // Register the settings-like entity types for queue-for-transfer, restore and
            // import/export. Per-node types are pulled in as content dependencies instead.
            foreach (var entityType in new[]
            {
                SeoToolkitDeployConstants.UdiEntityType.SeoSetting,
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsSetting,
                SeoToolkitDeployConstants.UdiEntityType.SitemapPageType,
                SeoToolkitDeployConstants.UdiEntityType.Script,
                SeoToolkitDeployConstants.UdiEntityType.DomainCollection,
                SeoToolkitDeployConstants.UdiEntityType.KeyValues,
            })
            {
                transferEntityService.RegisterTransferEntityType(
                    entityType,
                    new DeployRegisteredEntityTypeDetailOptions
                    {
                        SupportsQueueForTransfer = true,
                        SupportsRestore = true,
                        PermittedToRestore = true,
                        SupportsPartialRestore = true,
                        SupportsImportExport = true,
                    });
            }
        }
    }
}
