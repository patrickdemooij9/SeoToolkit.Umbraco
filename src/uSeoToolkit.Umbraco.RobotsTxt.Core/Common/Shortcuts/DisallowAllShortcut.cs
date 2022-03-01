using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.PropertyEditors;
using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Common.Shortcuts
{
    public class DisallowAllShortcut : IRobotsTxtShortcut
    {
        public string Name => "Disallow all";
        public ConfigurationField[] Fields => Array.Empty<ConfigurationField>();
        public string Execute(Dictionary<string, string> values)
        {
            return "User-agent: */r/nDisallow: /";
        }
    }
}
