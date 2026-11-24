using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeoToolkit.Umbraco.Common.Core.Enums;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.Common.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Dashboard
{
    /// <summary>
    /// The scripts the script manager adds to every page (the scripts that aren't domain specific).
    /// </summary>
    public class ScriptsDataSource : IDashboardDataSource
    {
        private readonly IScriptManagerService _scriptManagerService;

        public ScriptsDataSource(IScriptManagerService scriptManagerService)
        {
            _scriptManagerService = scriptManagerService;
        }

        public string Alias => "scriptManager.scripts";

        public Task<DashboardData> GetAsync(IReadOnlyDictionary<string, string> parameters)
        {
            var scripts = _scriptManagerService.GetAll(null).ToArray();

            var data = new DashboardData
            {
                Status = DashboardDataStatus.None,
                Value = scripts.Length
            };
            data.Items.AddRange(scripts.Select(it => new DashboardDataItem { Label = it.Name, Text = it.Definition?.Name }));

            return Task.FromResult(data);
        }
    }
}
