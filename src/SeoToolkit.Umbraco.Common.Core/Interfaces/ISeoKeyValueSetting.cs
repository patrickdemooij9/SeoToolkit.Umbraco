using System;
using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface ISeoKeyValueSetting
    {
        public string Key { get; }
        public string Title { get; }
        public string Description { get; }
        public string PropertyAlias { get; }

        public Type EditorType { get; }

        /// <summary>
        /// Optional configuration passed to the property editor that renders this setting.
        /// </summary>
        public IReadOnlyDictionary<string, object> EditConfig => null;

        /// <summary>
        /// When <c>true</c>, this setting is only shown on the root settings node and not on
        /// individual domain nodes.
        /// </summary>
        public bool RootOnly => false;
    }
}
