using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Models.Business
{
    public class SeoDomainCollection
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<int> DomainIds { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public SeoDomainCollection()
        {
            DomainIds = [];
            Settings = [];
        }

        public bool HasFunctionality(string key)
        {
            return Settings.ContainsKey(key) && Settings[key] == "true";
        }
    }
}
