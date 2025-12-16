using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Events;
using Umbraco.Deploy.Core.Events;

namespace SeoToolkit.Umbraco.Deploy.NotificationHandlers
{
    internal class ArtifactExportingNotificationHandler : INotificationAsyncHandler<ArtifactExportingNotification>
    {
        public Task HandleAsync(ArtifactExportingNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
