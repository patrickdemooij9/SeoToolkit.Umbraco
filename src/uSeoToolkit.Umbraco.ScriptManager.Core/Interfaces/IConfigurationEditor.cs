using System.Collections.Generic;

namespace uSeoToolkit.Umbraco.ScriptManager.Core.Interfaces
{
    public interface IConfigurationEditor
    {
        object FromConfigurationEditor(Dictionary<string, object> values);
    }
}
