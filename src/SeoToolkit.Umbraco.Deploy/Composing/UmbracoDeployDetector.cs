using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SeoToolkit.Umbraco.Deploy.Composing
{
    /// <summary>
    /// Detects whether Umbraco Deploy itself is installed. This package only references
    /// Umbraco.Deploy.Infrastructure; the services it depends on (IDiskEntityService,
    /// IServiceConnectorFactory, ITransferQueue, ...) are registered by the OnPrem or Cloud
    /// composer. Those assemblies can't be referenced here, so look the composers up by name —
    /// which, unlike inspecting the service collection, doesn't depend on composer order.
    /// </summary>
    internal static class UmbracoDeployDetector
    {
        private static readonly string[] DeployComposerTypeNames =
        [
            "Umbraco.Deploy.OnPrem.UmbracoDeployOnPremComposer",
            "Umbraco.Deploy.Cloud.UmbracoDeployCloudComposer",
        ];

        public static bool IsInstalled(IUmbracoBuilder builder)
            => builder.TypeLoader.GetTypes<IComposer>()
                .Any(type => DeployComposerTypeNames.Contains(type.FullName));
    }
}
