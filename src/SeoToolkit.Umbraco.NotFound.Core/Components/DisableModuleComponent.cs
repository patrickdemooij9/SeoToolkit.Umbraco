using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Enums;

namespace SeoToolkit.Umbraco.NotFound.Core.Components
{
    internal class DisableModuleComponent : IComponent
    {
        private readonly ModuleCollection _collection;

        public DisableModuleComponent(ModuleCollection collection)
        {
            _collection = collection;
        }

        public void Initialize()
        {
            _collection.SetStatus(NotFoundConstants.NotFoundModuleAlias, SeoToolkitModuleStatus.Disabled);
        }

        public void Terminate()
        {
        }
    }
}