using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Enums;
using System.Threading.Tasks;
using System.Threading;

namespace SeoToolkit.Umbraco.NotFound.Core.Components
{
    internal class DisableModuleComponent : IAsyncComponent
    {
        private readonly ModuleCollection _collection;

        public DisableModuleComponent(ModuleCollection collection)
        {
            _collection = collection;
        }

        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            _collection.SetStatus(NotFoundConstants.NotFoundModuleAlias, SeoToolkitModuleStatus.Disabled);
            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}