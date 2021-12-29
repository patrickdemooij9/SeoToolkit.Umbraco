using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Core.Collections;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Components
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
            _collection.EnableModule("metaFields");
        }

        public void Terminate()
        {
        }
    }
}
