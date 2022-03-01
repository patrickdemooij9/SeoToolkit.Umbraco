using uSeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Models.ViewModels
{
    public class RobotsTxtShortcutViewModel
    {
        public string Name { get; set; }
        public string Alias { get; set; }

        public RobotsTxtShortcutViewModel(IRobotsTxtShortcut shortcut)
        {
            Name = shortcut.Name;
            Alias = shortcut.Alias;
        }
    }
}
