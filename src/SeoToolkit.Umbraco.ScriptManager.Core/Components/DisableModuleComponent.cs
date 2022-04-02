using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;
using SeoToolkit.Umbraco.Common.Core.Enums;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Components
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
            _collection.SetStatus("scriptManager", SeoToolkitModuleStatus.Disabled);
        }

        public void Terminate()
        {
        }
    }
}
