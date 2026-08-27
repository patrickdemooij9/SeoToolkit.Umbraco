#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeoToolkit.Umbraco.SiteAudit.Core.Crawling
{
    /// <summary>
    /// Parses robots.txt and answers whether a given user agent may fetch a given path.
    /// <para>
    /// The toolkit already ships a robots.txt validator, but that checks syntax; a crawler needs
    /// group selection and longest-match precedence, which is a different problem. Parsing here
    /// also avoids taking a dependency on the RobotsTxt package - and matters for decoupled
    /// sites, where the robots.txt that governs the crawl is the one served by the frontend host
    /// rather than anything Umbraco knows about.
    /// </para>
    /// </summary>
    public sealed class RobotsTxtMatcher
    {
        private sealed class Group
        {
            public List<string> UserAgents { get; } = new();
            public List<(string Path, bool Allow)> Rules { get; } = new();
            public int? CrawlDelaySeconds { get; set; }
        }

        private readonly List<Group> _groups;

        /// <summary>Sitemap urls declared in the file, which are also crawl seeds.</summary>
        public IReadOnlyList<string> Sitemaps { get; }

        private RobotsTxtMatcher(List<Group> groups, List<string> sitemaps)
        {
            _groups = groups;
            Sitemaps = sitemaps;
        }

        /// <summary>A matcher that allows everything, used when robots.txt is absent or unreadable.</summary>
        public static RobotsTxtMatcher AllowAll { get; } = new(new List<Group>(), new List<string>());

        public static RobotsTxtMatcher Parse(string? content)
        {
            var groups = new List<Group>();
            var sitemaps = new List<string>();

            if (string.IsNullOrWhiteSpace(content))
                return AllowAll;

            Group? current = null;
            var lastLineWasUserAgent = false;

            foreach (var rawLine in content.Split('\n'))
            {
                var line = StripComment(rawLine).Trim();
                if (line.Length == 0) continue;

                var separator = line.IndexOf(':');
                if (separator <= 0) continue;

                var field = line[..separator].Trim().ToLowerInvariant();
                var value = line[(separator + 1)..].Trim();

                switch (field)
                {
                    case "user-agent":
                        // Consecutive user-agent lines share one group of rules.
                        if (current is null || !lastLineWasUserAgent)
                        {
                            current = new Group();
                            groups.Add(current);
                        }
                        current.UserAgents.Add(value.ToLowerInvariant());
                        lastLineWasUserAgent = true;
                        continue;

                    case "disallow":
                        current?.Rules.Add((value, false));
                        break;

                    case "allow":
                        current?.Rules.Add((value, true));
                        break;

                    case "crawl-delay":
                        if (current is not null && int.TryParse(value, out var delay))
                            current.CrawlDelaySeconds = delay;
                        break;

                    case "sitemap":
                        if (!string.IsNullOrWhiteSpace(value)) sitemaps.Add(value);
                        break;
                }

                lastLineWasUserAgent = false;
            }

            return new RobotsTxtMatcher(groups, sitemaps);
        }

        public bool IsAllowed(string userAgent, string path)
        {
            var group = SelectGroup(userAgent);
            if (group is null) return true;

            // Longest match wins, and Allow beats Disallow at equal length. This is what lets a
            // site say "block /admin but allow /admin/public" and have it mean what it looks like.
            var bestLength = -1;
            var allowed = true;

            foreach (var (pattern, allow) in group.Rules)
            {
                // An empty Disallow means "nothing is disallowed" and carries no precedence.
                if (pattern.Length == 0) continue;

                if (!Matches(pattern, path)) continue;

                var length = EffectiveLength(pattern);
                if (length > bestLength || (length == bestLength && allow))
                {
                    bestLength = length;
                    allowed = allow;
                }
            }

            return allowed;
        }

        public int? GetCrawlDelaySeconds(string userAgent) => SelectGroup(userAgent)?.CrawlDelaySeconds;

        /// <summary>
        /// Picks the group governing a user agent: the most specific name match, falling back to
        /// the wildcard group. A crawler must obey exactly one group, not the union of all of them.
        /// </summary>
        private Group? SelectGroup(string userAgent)
        {
            var agent = (userAgent ?? string.Empty).ToLowerInvariant();

            Group? best = null;
            var bestLength = -1;

            foreach (var group in _groups)
            {
                foreach (var candidate in group.UserAgents)
                {
                    if (candidate == "*") continue;
                    if (!agent.Contains(candidate, StringComparison.Ordinal)) continue;

                    if (candidate.Length > bestLength)
                    {
                        bestLength = candidate.Length;
                        best = group;
                    }
                }
            }

            return best ?? _groups.FirstOrDefault(it => it.UserAgents.Contains("*"));
        }

        /// <summary>Wildcards do not count towards specificity, only the literal characters do.</summary>
        private static int EffectiveLength(string pattern) => pattern.Count(c => c != '*');

        /// <summary>
        /// Prefix match with * as "any run of characters" and a trailing $ meaning "ends here".
        /// </summary>
        private static bool Matches(string pattern, string path)
        {
            var anchored = pattern.EndsWith('$');
            if (anchored) pattern = pattern[..^1];

            if (pattern.Length == 0) return !anchored || path.Length == 0;

            var segments = pattern.Split('*');
            var position = 0;

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment.Length == 0) continue;

                if (i == 0)
                {
                    // The first segment is anchored at the start of the path.
                    if (!path.StartsWith(segment, StringComparison.Ordinal)) return false;
                    position = segment.Length;
                    continue;
                }

                var found = path.IndexOf(segment, position, StringComparison.Ordinal);
                if (found < 0) return false;
                position = found + segment.Length;
            }

            if (!anchored) return true;

            // With $ the final segment has to sit at the very end.
            var lastSegment = segments[^1];
            return lastSegment.Length == 0
                ? position == path.Length
                : path.EndsWith(lastSegment, StringComparison.Ordinal);
        }

        private static string StripComment(string line)
        {
            var hash = line.IndexOf('#');
            return hash < 0 ? line : line[..hash];
        }
    }
}
