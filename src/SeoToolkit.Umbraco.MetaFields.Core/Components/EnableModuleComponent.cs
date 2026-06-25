using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;
using System.Threading;

namespace SeoToolkit.Umbraco.MetaFields.Core.Components
{
    internal class EnableModuleComponent : IAsyncComponent
    {
        private readonly ModuleCollection _collection;

        public EnableModuleComponent(ModuleCollection collection)
        {
            _collection = collection;
        }

        public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            _collection.EnableModule("metaFields");
            return Task.CompletedTask;
        }

        public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
