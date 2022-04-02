using System.Collections.Generic;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Converters;

namespace SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField
{
    public interface ISeoFieldEditor
    {
        string View { get; }
        Dictionary<string, object> Config { get; }
        IEditorValueConverter ValueConverter { get; }
    }
}
