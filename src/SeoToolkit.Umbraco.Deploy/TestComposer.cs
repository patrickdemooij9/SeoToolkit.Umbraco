using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Deploy.Infrastructure.Transfer;

namespace SeoToolkit.Umbraco.Deploy
{
    public class TestComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Components().Append<TestComponent>();
        }
    }

    public class TestComponent : IAsyncComponent
    {
        private readonly ITransferEntityService _transferEntityService;

        public TestComponent(ITransferEntityService transferEntityService)
        {
            _transferEntityService = transferEntityService;
        }

        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            _transferEntityService.RegisterTransferEntityType("seoToolkit-robotstxt", new DeployRegisteredEntityTypeDetailOptions
            {
                SupportsQueueForTransfer = true,
                SupportsQueueForTransferOfDescendents = true,
                SupportsRestore = true,
                PermittedToRestore = true,
                SupportsPartialRestore = true,
                SupportsImportExport = true,
            });
            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
