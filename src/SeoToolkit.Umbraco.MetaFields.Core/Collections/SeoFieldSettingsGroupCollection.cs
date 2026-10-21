using System;
using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Cms.Core.Composing;

namespace SeoToolkit.Umbraco.MetaFields.Core.Collections;

public class SeoFieldSettingsGroupCollection : BuilderCollectionBase<ISeoFieldSettingsGroup>
{
    public SeoFieldSettingsGroupCollection(Func<IEnumerable<ISeoFieldSettingsGroup>> items) : base(items)
    {
    }
}
