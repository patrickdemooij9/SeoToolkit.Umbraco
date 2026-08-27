#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions;
using SeoToolkit.Umbraco.SiteAudit.Core.Crawling;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Audit;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Services
{
    public interface ISeoCheckCatalogue
    {
        IReadOnlyList<SeoCheckDescriptor> GetAll();

        SeoCheckDescriptor? Get(string alias);

        /// <summary>
        /// Whether this installation can actually run the check. A check belonging to a feature
        /// that is not installed, or needing something the crawler cannot supply, is listed but
        /// never runs - saying so is better than quietly reporting it as passing.
        /// </summary>
        bool IsAvailable(SeoCheckDescriptor descriptor);

        /// <summary>Turns a stored finding into a sentence.</summary>
        string Describe(AuditIssue issue);
    }

    /// <summary>
    /// The list of checks this installation knows about, and the place stored findings are
    /// turned back into readable messages.
    /// <para>
    /// Formatting belongs here rather than in the crawler because issues are stored as structured
    /// data, not as finished sentences - which is what makes them translatable and what lets the
    /// results grid filter on real values.
    /// </para>
    /// </summary>
    public class SeoCheckCatalogue : ISeoCheckCatalogue
    {
        private readonly ICrawlEngineFactory _engineFactory;
        private readonly ISeoCheckMessageFormatter _formatter;

        public SeoCheckCatalogue(ICrawlEngineFactory engineFactory, ISeoCheckMessageFormatter formatter)
        {
            _engineFactory = engineFactory;
            _formatter = formatter;
        }

        public IReadOnlyList<SeoCheckDescriptor> GetAll()
            => _engineFactory.GetChecks().Select(it => it.Descriptor).ToArray();

        public SeoCheckDescriptor? Get(string alias)
        {
            if (string.IsNullOrEmpty(alias)) return null;

            return _engineFactory.GetChecks()
                .Select(it => it.Descriptor)
                .FirstOrDefault(it => string.Equals(it.Alias, alias, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsAvailable(SeoCheckDescriptor descriptor)
        {
            if (descriptor is null) return false;

            if (descriptor.RequiresFeature is not null && !SeoFeatureRegistry.IsEnabled(descriptor.RequiresFeature))
                return false;

            var available = _engineFactory.GetAvailableCapabilities();
            return (descriptor.RequiredCapabilities & ~available) == SeoCheckCapabilities.None;
        }

        public string Describe(AuditIssue issue)
        {
            if (issue is null) throw new ArgumentNullException(nameof(issue));

            var descriptor = Get(issue.CheckAlias);
            if (descriptor is null)
            {
                // The check that produced this is no longer installed. The finding is still real,
                // so show what is known rather than dropping the row.
                return issue.Evidence ?? issue.CheckAlias;
            }

            return _formatter.Format(ToCheckIssue(issue), descriptor);
        }

        /// <summary>
        /// Rebuilds the in-memory issue the formatter expects from its stored form.
        /// </summary>
        private static SeoCheckIssue ToCheckIssue(AuditIssue issue)
        {
            var result = new SeoCheckIssue(issue.CheckAlias, issue.Severity, issue.Variant, issue.Url);

            foreach (var pair in issue.Data) result.AddData(pair.Key, pair.Value);
            result.Evidence = issue.Evidence;

            return result;
        }
    }
}
