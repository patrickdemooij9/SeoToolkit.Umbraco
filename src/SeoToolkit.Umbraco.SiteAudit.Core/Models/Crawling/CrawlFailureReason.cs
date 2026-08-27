#nullable enable
namespace SeoToolkit.Umbraco.SiteAudit.Core.Models.Crawling
{
    /// <summary>
    /// Why a resource could not be fetched. A failure is distinct from an error status code:
    /// a 404 is a successful request with a bad answer, a DNS failure is no answer at all.
    /// </summary>
    public enum CrawlFailureReason
    {
        None = 0,
        Timeout = 1,
        DnsFailure = 2,
        ConnectionFailure = 3,
        CertificateFailure = 4,
        TooManyRedirects = 5,
        RedirectLoop = 6,
        ResponseTooLarge = 7,
        InvalidUrl = 8,
        Cancelled = 9,
        Unknown = 10
    }
}
