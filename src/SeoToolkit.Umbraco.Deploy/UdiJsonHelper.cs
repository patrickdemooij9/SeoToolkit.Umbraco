using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace SeoToolkit.Umbraco.Deploy
{
    public static partial class UdiJsonHelper
    {
        [GeneratedRegex(@"umb://(document|media|member)/([0-9a-fA-F]{32})")]
        private static partial Regex UdiRegex();

        /// <summary>
        /// Scans a JSON blob for embedded document/media/member UDIs so they can be added
        /// as artifact dependencies. Returns distinct UDIs.
        /// </summary>
        public static IEnumerable<Udi> FindUdis(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                yield break;
            }

            var seen = new HashSet<string>();
            foreach (Match match in UdiRegex().Matches(json))
            {
                if (seen.Add(match.Value) && UdiParser.TryParse(match.Value, out Udi? udi) && udi is not null)
                {
                    yield return udi;
                }
            }
        }
    }
}
