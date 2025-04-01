﻿using SeoToolkit.Umbraco.Common.Core.Collections;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.NotFound.Core.Components;

internal class EnableModuleComponent : IComponent
{
    private readonly ModuleCollection _collection;

    public EnableModuleComponent(ModuleCollection collection)
    {
        _collection = collection;
    }

    public void Initialize()
    {
        _collection.EnableModule(NotFoundConstants.NotFoundModuleAlias);

    }

    public void Terminate() { }
}