using System.Collections.Generic;
using uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldEditor
    {
        string View { get; }
        Dictionary<string, object> Config { get; }
        IEditorValueConverter ValueConverter { get; }
    }
}
