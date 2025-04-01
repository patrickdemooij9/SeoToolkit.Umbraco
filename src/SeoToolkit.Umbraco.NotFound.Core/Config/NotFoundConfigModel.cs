using System;

namespace SeoToolkit.Umbraco.NotFound.Core.Config
{
    public class NotFoundConfigModel
    {
        public string[] DisabledModules { get; set; } = Array.Empty<string>();
    }
}