using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.NotFound.Core.Services;
using SeoToolkit.Umbraco.NotFound.Core.Startup;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.NotFound.Core.ContentFinders;

public class PageNotFoundFinder : IContentLastChanceFinder
{
    private readonly IPageNotFoundService _pageNotFoundService;
    private readonly ISeoDomainResolver _seoDomainResolver;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public PageNotFoundFinder(IPageNotFoundService pageNotFoundService, ISeoDomainResolver seoDomainResolver, IUmbracoContextAccessor umbracoContextAccessor)
    {
        _pageNotFoundService = pageNotFoundService;
        _seoDomainResolver = seoDomainResolver;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    public Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        _umbracoContextAccessor.TryGetUmbracoContext(out var context);

        var seoDomain = _seoDomainResolver.ResolveDomain();
        if (seoDomain != null && !seoDomain.HasFunctionality($"Module.{NotFoundTreeSection.SectionGuid}"))
        {
            seoDomain = null;
        }

        var pageNotFoundGuid = _pageNotFoundService.GetPageNotFound(seoDomain?.Id);
        if (pageNotFoundGuid is null)
        {
            return Task.FromResult(false);
        }

        var page = context?.Content?.GetById(pageNotFoundGuid.Value);

        if (page == null || !page.IsPublished())
        {
            return Task.FromResult(false);
        }

        request.SetResponseStatus(404);
        request.SetPublishedContent(page);
        return Task.FromResult(true);
    }
}