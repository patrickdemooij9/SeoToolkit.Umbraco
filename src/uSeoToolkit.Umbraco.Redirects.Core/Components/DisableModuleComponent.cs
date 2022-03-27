using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Common.Core.Collections;
using uSeoToolkit.Umbraco.Common.Core.Enums;

namespace uSeoToolkit.Umbraco.Redirects.Core.Components
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
            _collection.SetStatus("redirects", SeoToolkitModuleStatus.Disabled);
        }

        public void Terminate()
        {
        }
    }
}
