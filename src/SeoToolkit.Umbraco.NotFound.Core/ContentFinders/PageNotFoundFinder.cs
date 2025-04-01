using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.NotFound.Core.ContentFinders;

public class PageNotFoundFinder : IContentLastChanceFinder
{
    private readonly IKeyValueService _keyValueService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    public PageNotFoundFinder(IKeyValueService keyValueService, IUmbracoContextAccessor umbracoContextAccessor)
    {
        _keyValueService = keyValueService;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    public Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        _umbracoContextAccessor.TryGetUmbracoContext(out var context);

        var pageNotFoundId = _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey);

        if (string.IsNullOrWhiteSpace(pageNotFoundId))
        {
            return Task.FromResult(false);
        }

        var page = context?.Content?.GetById(Convert.ToInt32(pageNotFoundId));

        if (page == null || !page.IsPublished())
        {
            return Task.FromResult(false);
        }

        request.SetResponseStatus(404);
        request.SetPublishedContent(page);
        return Task.FromResult(true);
    }
}