using SeoToolkit.Umbraco.Common.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoKeyValueSettingCollection : BuilderCollectionBase<ISeoKeyValueSetting>
    {
        public SeoKeyValueSettingCollection(Func<IEnumerable<ISeoKeyValueSetting>> items) : base(items)
        {
        }
    }
}
