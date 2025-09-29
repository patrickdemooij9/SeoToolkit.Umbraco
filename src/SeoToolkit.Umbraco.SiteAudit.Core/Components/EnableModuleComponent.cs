using Umbraco.Cms.Core.Composing;
using SeoToolkit.Umbraco.Common.Core.Collections;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Components
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
            _collection.EnableModule("siteAudit");
        }

        public void Terminate()
        {
        }
    }
}
