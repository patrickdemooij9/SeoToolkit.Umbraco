#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Checks.Abstractions
{
    /// <summary>
    /// Where reported issues go. Implemented by the crawler; checks never see it directly.
    /// May be called from several threads at once.
    /// </summary>
    public interface ISeoIssueSink
    {
        void Add(SeoCheckIssue issue);
    }
}
