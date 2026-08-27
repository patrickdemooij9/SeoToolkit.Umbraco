#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// How badly a failed check reflects on the site. Ordered so that comparisons work,
    /// and so the numeric value can be stored and filtered on directly.
    /// </summary>
    public enum SeoSeverity
    {
        /// <summary>The check ran and found nothing wrong. Not normally persisted.</summary>
        Passed = 0,
        /// <summary>Worth knowing about, but not a defect.</summary>
        Notice = 1,
        /// <summary>Should be looked at.</summary>
        Warning = 2,
        /// <summary>A real problem affecting how the page performs in search.</summary>
        Error = 3,
        /// <summary>Actively harmful, for example a page that cannot be indexed at all.</summary>
        Critical = 4
    }
}
