#nullable enable
using System;
using Newtonsoft.Json;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit
{
    /// <summary>
    /// The choices made when an audit was set up, stored with the run.
    /// <para>
    /// Kept as json on the run rather than as a join table. Checks are identified by alias now,
    /// so there is nothing to join to - and storing the configuration whole means a run can be
    /// repeated exactly, even if the set of installed checks has changed since.
    /// </para>
    /// </summary>
    public sealed class AuditRunConfig
    {
        /// <summary>
        /// Aliases of the checks to run. Empty means every enabled check, which is what a run
        /// created before this existed should fall back to.
        /// </summary>
        [JsonProperty("checks")]
        public string[] Checks { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The node the crawl was aimed at, when it was aimed at one rather than at a url.
        /// Recorded so the origin of a run stays traceable; nothing in the crawl depends on it,
        /// because a decoupled frontend may route in a way that no node url predicts.
        /// </summary>
        [JsonProperty("startNodeKey")]
        public Guid? StartNodeKey { get; set; }

        /// <summary>Which language of that node, when it is published in more than one.</summary>
        [JsonProperty("culture")]
        public string? Culture { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);

        public static AuditRunConfig FromJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new AuditRunConfig();

            try
            {
                return JsonConvert.DeserializeObject<AuditRunConfig>(json!) ?? new AuditRunConfig();
            }
            catch (JsonException)
            {
                // Unreadable configuration should not stop the run; falling back to every check
                // is the safer of the two wrong answers.
                return new AuditRunConfig();
            }
        }
    }
}
