using SeoToolkit.Umbraco.Common.Core.Interfaces;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.Common.Core.Collections
{
    public class SeoKeyValueSettingCollectionBuilder : WeightedCollectionBuilderBase<SeoKeyValueSettingCollectionBuilder, SeoKeyValueSettingCollection, ISeoKeyValueSetting>
    {
        protected override SeoKeyValueSettingCollectionBuilder This => this;
    }
}
