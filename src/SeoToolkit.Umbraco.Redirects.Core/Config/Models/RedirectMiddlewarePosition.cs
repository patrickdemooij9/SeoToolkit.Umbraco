namespace SeoToolkit.Umbraco.Redirects.Core.Config.Models
{
    /// <summary>
    /// Values according to Umbraco documentation for custom middleware: https://docs.umbraco.com/umbraco-cms/reference/routing/custom-middleware
    /// </summary>
    public enum RedirectMiddlewarePosition
    {
        PrePipeline,
        PreRouting,
        PostRouting,
        PostPipeline,
        Endpoints
    }
}