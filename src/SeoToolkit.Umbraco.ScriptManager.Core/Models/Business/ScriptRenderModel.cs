using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using SeoToolkit.Umbraco.ScriptManager.Core.Enums;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Business
{
    public class ScriptRenderModel
    {
        private Dictionary<ScriptPositionType, List<HtmlString>> Positions { get; }

        public ScriptRenderModel()
        {
            Positions = new Dictionary<ScriptPositionType, List<HtmlString>>();
        }

        public void AddScript(ScriptPositionType position, HtmlString script)
        {
            if (!Positions.ContainsKey(position))
                Positions.Add(position, new List<HtmlString>());
            Positions[position].Add(script);
        }

        public IEnumerable<HtmlString> Get(ScriptPositionType position)
        {
            return Positions.ContainsKey(position) ? Positions[position] : Enumerable.Empty<HtmlString>();
        }
    }
}
