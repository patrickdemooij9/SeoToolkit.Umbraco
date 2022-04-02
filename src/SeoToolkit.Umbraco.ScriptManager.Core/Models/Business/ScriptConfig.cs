using System.Collections.Generic;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Models.Business
{
    //TODO: See if we can make this strongly typed for each definition somehow...
    public class ScriptConfig
    {
        public Dictionary<string, string> Values { get; }

        public ScriptConfig()
        {
            Values = new Dictionary<string, string>();
        }

        public ScriptConfig(Dictionary<string, string> values)
        {
            Values = values;
        }

        public string GetValue(string key)
        {
            if (!Values.ContainsKey(key)) return default;
            return Values[key];
        }
    }
}
