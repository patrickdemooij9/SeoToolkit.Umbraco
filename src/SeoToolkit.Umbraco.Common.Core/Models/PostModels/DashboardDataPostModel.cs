using System.Collections.Generic;

namespace SeoToolkit.Umbraco.Common.Core.Models.PostModels
{
    public class DashboardDataPostModel
    {
        public string DataSource { get; set; } = string.Empty;
        public Dictionary<string, string>? Parameters { get; set; }
    }
}
