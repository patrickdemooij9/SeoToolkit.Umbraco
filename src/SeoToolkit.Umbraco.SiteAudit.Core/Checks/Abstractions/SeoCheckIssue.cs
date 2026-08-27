#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Something a check found wrong.
    /// <para>
    /// Issues carry structured data rather than a finished sentence. That is what makes
    /// localisation possible at all - the old api asked checks to format their own message,
    /// which baked English into the stored result - and it lets the results grid sort and
    /// filter on real values, for example ordering pages by title length, without asking
    /// the server to re-run anything.
    /// </para>
    /// </summary>
    public sealed class SeoCheckIssue
    {
        private Dictionary<string, string>? _data;
        private List<Uri>? _relatedUrls;
        private string? _issueHash;

        public SeoCheckIssue(string checkAlias, SeoSeverity severity, string? variant = null, Uri? url = null)
        {
            CheckAlias = checkAlias ?? throw new ArgumentNullException(nameof(checkAlias));
            Severity = severity;
            Variant = variant;
            Url = url;
        }

        public string CheckAlias { get; }

        /// <summary>
        /// Which flavour of problem this is, for checks that can fail in more than one way -
        /// "Missing" versus "TooLong", say. Selects the message template.
        /// </summary>
        public string? Variant { get; }

        public SeoSeverity Severity { get; internal set; }

        /// <summary>The affected url. Null means the issue is about the site as a whole.</summary>
        public Uri? Url { get; }

        /// <summary>Values behind the message, and what the results grid filters on.</summary>
        public IReadOnlyDictionary<string, string> Data
            => (IReadOnlyDictionary<string, string>?)_data ?? EmptyData;

        /// <summary>A short excerpt showing the problem, e.g. the offending title text.</summary>
        public string? Evidence { get; internal set; }

        /// <summary>Other urls involved, for example the rest of a set of duplicates.</summary>
        public IReadOnlyList<Uri> RelatedUrls
            => (IReadOnlyList<Uri>?)_relatedUrls ?? Array.Empty<Uri>();

        /// <summary>
        /// Distinguishes several issues raised by the same check on the same page - the
        /// individual broken link, for instance. Part of <see cref="IssueHash"/>.
        /// </summary>
        public string? Key { get; internal set; }

        /// <summary>
        /// Stable identity for this issue, used to recognise the same finding across runs.
        /// Replaces the old ISiteCheck.Compare, which every check had to implement and
        /// which nothing ever called.
        /// </summary>
        public string IssueHash => _issueHash ??= ComputeHash();

        internal void AddData(string key, string value)
        {
            _data ??= new Dictionary<string, string>(4, StringComparer.OrdinalIgnoreCase);
            _data[key] = value;
        }

        internal void AddRelatedUrl(Uri url)
        {
            _relatedUrls ??= new List<Uri>(2);
            _relatedUrls.Add(url);
        }

        private string ComputeHash()
        {
            var builder = new StringBuilder(128)
                .Append(CheckAlias).Append('|')
                .Append(Variant).Append('|')
                .Append(Url?.AbsoluteUri).Append('|')
                .Append(Key ?? Evidence);

            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static readonly IReadOnlyDictionary<string, string> EmptyData = new Dictionary<string, string>(0);

        internal static string FormatValue(object value) => value switch
        {
            null => string.Empty,
            string s => s,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
