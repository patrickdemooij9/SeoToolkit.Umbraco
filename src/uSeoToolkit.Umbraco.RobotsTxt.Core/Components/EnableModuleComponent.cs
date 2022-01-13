using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Core.Collections;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Components
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
            _collection.EnableModule("robotsTxt");
        }

        public void Terminate()
        {
        }
    }
}
