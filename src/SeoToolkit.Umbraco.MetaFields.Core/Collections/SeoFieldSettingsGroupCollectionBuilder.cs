using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections;

public class SeoFieldSettingsGroupCollectionBuilder : OrderedCollectionBuilderBase<SeoFieldSettingsGroupCollectionBuilder, SeoFieldSettingsGroupCollection, ISeoFieldSettingsGroup>
{
    protected override SeoFieldSettingsGroupCollectionBuilder This => this;
}
