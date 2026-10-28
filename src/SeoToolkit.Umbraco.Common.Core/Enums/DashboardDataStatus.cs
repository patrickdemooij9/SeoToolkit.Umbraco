using System.Text.Json.Serialization;

namespace SeoToolkit.Umbraco.Common.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DashboardDataStatus
    {
        None = 0,
        Positive = 1,
        Warning = 2,
        Danger = 3
    }
}
