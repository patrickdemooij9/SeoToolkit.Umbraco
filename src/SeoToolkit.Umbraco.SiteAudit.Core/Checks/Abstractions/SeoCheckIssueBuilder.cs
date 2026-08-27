#nullable enable
using System;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Fluent handle on an issue that has already been recorded.
    /// <para>
    /// The issue is added to the run the moment it is reported, so a check that ignores the
    /// return value still works. This is a struct wrapping the issue reference: reporting is
    /// rare - most pages pass most checks - and when it does happen the only allocation is
    /// the issue itself.
    /// </para>
    /// </summary>
    public readonly struct SeoCheckIssueBuilder
    {
        private readonly SeoCheckIssue? _issue;

        internal SeoCheckIssueBuilder(SeoCheckIssue? issue)
        {
            _issue = issue;
        }

        /// <summary>A builder that discards everything, returned when a check reports while disabled.</summary>
        public static SeoCheckIssueBuilder None => default;

        /// <summary>Adds a value behind the message, e.g. Data("Length", 93).</summary>
        public SeoCheckIssueBuilder Data(string key, object value)
        {
            _issue?.AddData(key, SeoCheckIssue.FormatValue(value));
            return this;
        }

        /// <summary>Adds a short excerpt showing the problem.</summary>
        public SeoCheckIssueBuilder Evidence(string? evidence)
        {
            if (_issue is not null) _issue.Evidence = evidence;
            return this;
        }

        /// <summary>
        /// Sets what makes this issue distinct from others the same check raised on the same
        /// page - the broken url, for example. Without it, several findings on one page would
        /// collapse into a single identity.
        /// </summary>
        public SeoCheckIssueBuilder Key(string? key)
        {
            if (_issue is not null) _issue.Key = key;
            return this;
        }

        /// <summary>Overrides the severity for this particular issue.</summary>
        public SeoCheckIssueBuilder Severity(SeoSeverity severity)
        {
            if (_issue is not null) _issue.Severity = severity;
            return this;
        }

        /// <summary>Links another url to this issue, for example the other pages sharing a duplicate title.</summary>
        public SeoCheckIssueBuilder Related(Uri url)
        {
            if (url is not null) _issue?.AddRelatedUrl(url);
            return this;
        }
    }
}
