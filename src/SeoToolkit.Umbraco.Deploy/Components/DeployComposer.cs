using SeoToolkit.Umbraco.Deploy.NotificationHandlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Deploy.Core.Events;

namespace SeoToolkit.Umbraco.Deploy.Components
{
    public class DeployComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddComponent<DeployComponent>();

            builder.AddNotificationAsyncHandler<ArtifactExportingNotification, ArtifactExportingNotificationHandler>();
        }
    }
}
