using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors;
using Umbraco.Deploy.Infrastructure.Transfer;

namespace SeoToolkit.Umbraco.Deploy.Components
{
    public class DeployComponent : IAsyncComponent
    {
        private readonly IServiceConnectorFactory _serviceConnectorFactory;
        private readonly ITransferEntityService _transferEntityService;

        public DeployComponent(IServiceConnectorFactory serviceConnectorFactory, ITransferEntityService transferEntityService)
        {
            _serviceConnectorFactory = serviceConnectorFactory;
            _transferEntityService = transferEntityService;
        }

        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            // Let Umbraco & Deploy know about a new UDI type
            // umb://seotoolkit-robotstxt/someguid-without-hyphens
            UdiParser.RegisterUdiType("seoToolkit-robotstxt", UdiType.GuidUdi);

            // Allow RobotsTxt to be transferred via UI and not schema/disk-entity
            _transferEntityService.RegisterTransferEntityType("seoToolkit-robotstxt", new DeployRegisteredEntityTypeDetailOptions
            {
                SupportsQueueForTransfer = true,
                PermittedToRestore = true,
                // SupportsExportOfDescendants = false,
                SupportsImportExport = true,
                SupportsRestore = true,
                SupportsPartialRestore = true
            });

            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
