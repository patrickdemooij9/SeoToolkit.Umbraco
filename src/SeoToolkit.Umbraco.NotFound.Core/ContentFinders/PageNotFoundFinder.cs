using SeoToolkit.Umbraco.Common.Core.Helpers;
using SeoToolkit.Umbraco.NotFound.Core.Services;
using SeoToolkit.Umbraco.NotFound.Core.Startup;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.NotFound.Core.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.NotFound.Core.ContentFinders;

public class PageNotFoundFinder : IContentLastChanceFinder
{
    private readonly IPageNotFoundService _pageNotFoundService;
    private readonly ISeoDomainResolver _seoDomainResolver;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IEventAggregator _eventAggregator;

    public PageNotFoundFinder(IPageNotFoundService pageNotFoundService, ISeoDomainResolver seoDomainResolver, IUmbracoContextAccessor umbracoContextAccessor, IEventAggregator eventAggregator)
    {
        _pageNotFoundService = pageNotFoundService;
        _seoDomainResolver = seoDomainResolver;
        _umbracoContextAccessor = umbracoContextAccessor;
        _eventAggregator = eventAggregator;
    }

    public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
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
            return false;
        }

        var page = context?.Content?.GetById(pageNotFoundGuid.Value);

        // Fire notification so the page can be changed before returning
        var notification = new PageNotFoundNotification(page);
        await _eventAggregator.PublishAsync(notification);

        if (notification.Page == null || !notification.Page.IsPublished())
        {
            return false;
        }

        request.SetResponseStatus(404);
        request.SetPublishedContent(notification.Page);
        return true;
    }
}