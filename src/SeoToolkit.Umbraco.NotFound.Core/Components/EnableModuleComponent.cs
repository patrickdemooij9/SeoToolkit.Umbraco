﻿using SeoToolkit.Umbraco.Common.Core.Collections;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.NotFound.Core.Components;

internal class EnableModuleComponent : IAsyncComponent
{
    private readonly ModuleCollection _collection;

    public EnableModuleComponent(ModuleCollection collection)
    {
        _collection = collection;
    }

    public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        _collection.EnableModule(NotFoundConstants.NotFoundModuleAlias);
        return Task.CompletedTask;
    }

    public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}