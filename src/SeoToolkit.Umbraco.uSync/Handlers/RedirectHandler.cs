using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.Redirects.Core.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Strings;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.BackOffice.SyncHandlers.Interfaces;
using uSync.BackOffice.SyncHandlers.Models;
using uSync.Core;

namespace SeoToolkit.Umbraco.uSync.Handlers
{
    /*[SyncHandler("RedirectHandler", "RedirectHandler", "SeoToolkitRedirects", 3100, Icon = "icon-shield")]
    public class RedirectHandler : SyncHandlerRoot<Redirect, Redirect>, ISyncHandler,
        INotificationAsyncHandler<RedirectSavedNotification>,
        INotificationAsyncHandler<RedirectDeletedNotification>
    {
        private readonly IRedirectsService _redirectsService;

        public RedirectHandler(ILogger<SyncHandlerRoot<Redirect, Redirect>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, ISyncFileService syncFileService, ISyncEventService mutexService, ISyncConfigService uSyncConfig, ISyncItemFactory itemFactory, IRedirectsService redirectsService) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _redirectsService = redirectsService;
        }

        public Task HandleAsync(RedirectDeletedNotification notification, CancellationToken cancellationToken)
        {
            return HandleEventAsync(notification.Model, cancellationToken);
        }

        public Task HandleAsync(RedirectSavedNotification notification, CancellationToken cancellationToken)
        {
            return HandleEventAsync(notification.Model, cancellationToken);
        }

        public async Task HandleEventAsync(Redirect item, CancellationToken cancellationToken)
        {
            try
            {
                var handlerFolders = GetDefaultHandlerFolders();
                var attempts = await ExportAsync(item, handlerFolders, DefaultConfig);
                foreach (var attempt in attempts)
                {
                    if (attempt.Success && attempt.FileName is not null)
                    {
                        await CleanUpAsync(item, attempt.FileName, handlerFolders[handlerFolders.Length - 1]);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to create uSync export file");
            }
        }

        protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(Redirect parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return Task.FromResult(Enumerable.Empty<uSyncAction>());
        }

        protected override Task<IEnumerable<Redirect>> GetChildItemsAsync(Redirect? parent)
        {
            if (parent is not null)
            {
                return Task.FromResult(Enumerable.Empty<Redirect>());
            }
            return Task.FromResult(_redirectsService.GetAll(1, int.MaxValue).Items!);
        }

        protected override Task<IEnumerable<Redirect>> GetFoldersAsync(Redirect? parent)
        {
            return Task.FromResult(Enumerable.Empty<Redirect>());
        }

        protected override Task<Redirect?> GetFromServiceAsync(Redirect? item)
        {
            if (item is null)
                return Task.FromResult<Redirect?>(null);
            return Task.FromResult(_redirectsService.Get(item.Key));
        }

        protected override string GetItemName(Redirect item)
        {
            return $"Redirect {item.OldUrl}";
        }
    }*/
}
