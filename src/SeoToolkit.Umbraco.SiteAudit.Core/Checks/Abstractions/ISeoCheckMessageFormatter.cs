#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Turns a stored issue into a sentence.
    /// <para>
    /// Formatting happens here, at read time, rather than in the check. That is the whole
    /// reason issues carry structured data: the old api had each check build its own message
    /// during the crawl, which meant the wording was frozen into the database in one language
    /// and could never be translated or reworded afterwards.
    /// </para>
    /// </summary>
    public interface ISeoCheckMessageFormatter
    {
        string Format(SeoCheckIssue issue, SeoCheckDescriptor descriptor);
    }
}
