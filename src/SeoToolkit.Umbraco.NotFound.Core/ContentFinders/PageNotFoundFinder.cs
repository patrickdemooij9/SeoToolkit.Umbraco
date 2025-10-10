using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SeoToolkit.Umbraco.NotFound.Core.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace SeoToolkit.Umbraco.NotFound.Core.ContentFinders;

public class PageNotFoundFinder : IContentLastChanceFinder
{
    private readonly IKeyValueService _keyValueService;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IEventAggregator _eventAggregator;

    public PageNotFoundFinder(
        IKeyValueService keyValueService, 
        IUmbracoContextAccessor umbracoContextAccessor,
        IEventAggregator eventAggregator)
    {
        _keyValueService = keyValueService;
        _umbracoContextAccessor = umbracoContextAccessor;
        _eventAggregator = eventAggregator;
    }

    public async Task<bool> TryFindContent(IPublishedRequestBuilder request)
    {
        _umbracoContextAccessor.TryGetUmbracoContext(out var context);

        var pageNotFoundId = _keyValueService.GetValue(NotFoundConstants.NotFoundKeyValueKey);

        if (string.IsNullOrWhiteSpace(pageNotFoundId) || !Guid.TryParse(pageNotFoundId, out var pageNotFoundGuid))
        {
            return false;
        }

        var page = context?.Content?.GetById(pageNotFoundGuid);

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