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

            // Per-node types (MetaFieldsValue / SitemapContent) are content data, not schema, so
            // they are not registered as disk entity types.
        }

        private void InitializeIntegratedEntities()
        {
            // Settings-like entity types: queue-for-transfer, restore and disk import/export.
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

            // Per-node types: transfer/restore entities, but SupportsImportExport is false so they
            // are never written to disk as schema .uda (their signature is refreshed on change).
            foreach (var entityType in new[]
            {
                SeoToolkitDeployConstants.UdiEntityType.MetaFieldsValue,
                SeoToolkitDeployConstants.UdiEntityType.SitemapContent,
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
                        SupportsImportExport = false,
                    });
            }
        }
    }
}
