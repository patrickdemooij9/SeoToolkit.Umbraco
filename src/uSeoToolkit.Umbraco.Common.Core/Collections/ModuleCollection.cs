using System.Collections.Generic;
using System.Linq;
using uSeoToolkit.Umbraco.Common.Core.Enums;
using uSeoToolkit.Umbraco.Common.Core.Models;

namespace uSeoToolkit.Umbraco.Common.Core.Collections
{
    public class ModuleCollection
    {
        private readonly List<SeoToolkitModule> _items;

        public ModuleCollection()
        {
            _items = new List<SeoToolkitModule>
            {
                new SeoToolkitModule
                {
                    Title = "Meta Fields",
                    Alias = "metaFields",
                    Icon = "icon-thumbnail-list",
                    Link = "https://useotoolkit.gitbook.io/useotoolkit/getting-started/meta-fields"
                },
                new SeoToolkitModule
                {
                    Title = "Script Manager",
                    Alias = "scriptManager",
                    Icon = "icon-script",
                    Link = "https://useotoolkit.gitbook.io/useotoolkit/getting-started/script-manager"
                },
                new SeoToolkitModule
                {
                    Title = "Sitemap",
                    Alias = "sitemap",
                    Icon = "icon-sitemap",
                    Link = "https://useotoolkit.gitbook.io/useotoolkit/getting-started/sitemap"
                },
                new SeoToolkitModule
                {
                    Title = "Robots.txt",
                    Alias = "robotsTxt",
                    Icon = "icon-cloud",
                    Link = "https://useotoolkit.gitbook.io/useotoolkit/getting-started/robots.txt"
                },
                new SeoToolkitModule
                {
                    Title = "Redirects",
                    Alias = "redirects",
                    Icon = "icon-trafic",
                    Link = "https://useotoolkit.gitbook.io/useotoolkit/getting-started/redirects"
                }
            };
        }

        public IEnumerable<SeoToolkitModule> GetAll() => _items;

        public void EnableModule(string alias)
        {
            SetStatus(alias, SeoToolkitModuleStatus.Installed);
        }

        public void SetStatus(string alias, SeoToolkitModuleStatus status)
        {
            var module = _items.FirstOrDefault(it => it.Alias == alias);
            if (module != null)
                module.Status = status;
        }
    }
}
