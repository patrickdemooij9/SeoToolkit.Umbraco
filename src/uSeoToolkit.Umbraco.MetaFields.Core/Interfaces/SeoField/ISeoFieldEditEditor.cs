using System;
using System.Collections.Generic;
using System.Text;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldEditEditor
    {
        string View { get; }
        Dictionary<string, object> Config { get; }
        IEditorValueConverter ValueConverter { get; }
    }
}
