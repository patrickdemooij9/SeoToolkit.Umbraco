using System.Collections.Generic;
using Umbraco.Cms.Core.PropertyEditors;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces
{
    public interface IRobotsTxtShortcut
    {
        string Name { get; }
        string Alias { get; }
        ConfigurationField[] Fields { get; }

        string Execute(Dictionary<string, string> values);
    }
}
