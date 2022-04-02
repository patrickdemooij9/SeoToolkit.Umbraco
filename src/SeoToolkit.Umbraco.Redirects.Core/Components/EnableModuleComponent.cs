using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;

namespace SeoToolkit.Umbraco.Redirects.Core.Components
{
    internal class EnableModuleComponent : IComponent
    {
        private readonly ModuleCollection _collection;

        public EnableModuleComponent(ModuleCollection collection)
        {
            _collection = collection;
        }

        public void Initialize()
        {
            _collection.EnableModule("redirects");
        }

        public void Terminate()
        {
        }
    }
}
