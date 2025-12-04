using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Sitemap.Core.Models.Business;
using SeoToolkit.Umbraco.Sitemap.Core.Notifications;
using SeoToolkit.Umbraco.Sitemap.Core.Services.SitemapService;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
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
    [SyncHandler("SitemapSettingsHandler", "Sitemap Settings", "SeoToolkit//SitemapSettings", 3200, Icon = "icon-shield")]
    public class SitemapSettingsHandler : SyncHandlerRoot<SitemapPageSettings, SitemapPageSettings>, ISyncHandler,
        INotificationAsyncHandler<SitemapPageSettingsSavedNotification>
    {
        private readonly ISitemapService _sitemapService;
        private readonly IContentTypeService _contentTypeService;

        public SitemapSettingsHandler(ILogger<SyncHandlerRoot<SitemapPageSettings, SitemapPageSettings>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, ISyncFileService syncFileService, ISyncEventService mutexService, ISyncConfigService uSyncConfig, ISyncItemFactory itemFactory, ISitemapService sitemapService, IContentTypeService contentTypeService) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _sitemapService = sitemapService;
            _contentTypeService = contentTypeService;
        }

        public async Task HandleAsync(SitemapPageSettingsSavedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var handlerFolders = GetDefaultHandlerFolders();
                var item = notification.Model;
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

        protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(SitemapPageSettings parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return Task.FromResult(Enumerable.Empty<uSyncAction>());
        }

        protected override Task<IEnumerable<SitemapPageSettings>> GetChildItemsAsync(SitemapPageSettings? parent)
        {
            if (parent != null)
            {
                return Task.FromResult(Enumerable.Empty<SitemapPageSettings>());
            }
            return Task.FromResult<IEnumerable<SitemapPageSettings>>(_sitemapService.GetAll());
        }

        protected override Task<IEnumerable<SitemapPageSettings>> GetFoldersAsync(SitemapPageSettings? parent)
        {
            return Task.FromResult(Enumerable.Empty<SitemapPageSettings>());
        }

        protected override Task<SitemapPageSettings?> GetFromServiceAsync(SitemapPageSettings? item)
        {
            if (item is null) return Task.FromResult<SitemapPageSettings?>(null);
            return Task.FromResult(_sitemapService.GetPageTypeSettings(item.ContentTypeGuid));
        }

        protected override string GetItemName(SitemapPageSettings item)
        {
            return _contentTypeService.Get(item.ContentTypeGuid)?.Name ?? item.ContentTypeGuid.ToString();
        }
    }
}
