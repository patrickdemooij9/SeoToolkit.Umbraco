using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IConfigurationEditor
    {
        object FromConfigurationEditor(Dictionary<string, object> values);
    }
}
