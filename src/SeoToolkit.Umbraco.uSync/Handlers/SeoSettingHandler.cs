using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Notifications;
using SeoToolkit.Umbraco.Common.Core.Services.SeoSettingsService;
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
    [SyncHandler("SeoSettingsHandler", "Seo Settings", "SeoToolkit//SeoSetting", 3000, Icon = "icon-globe")]
    public class SeoSettingHandler : SyncHandlerRoot<SeoSetting, SeoSetting>, ISyncHandler, INotificationAsyncHandler<SeoSettingSavedNotification>
    {
        private readonly ISeoSettingsService _seoSettingsService;
        private readonly IContentTypeService _contentTypeService;

        public SeoSettingHandler(ILogger<SyncHandlerRoot<SeoSetting, SeoSetting>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, ISyncFileService syncFileService, ISyncEventService mutexService, ISyncConfigService uSyncConfig, ISyncItemFactory itemFactory, ISeoSettingsService seoSettingsService, IContentTypeService contentTypeService) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _seoSettingsService = seoSettingsService;
            _contentTypeService = contentTypeService;
        }

        public async Task HandleAsync(SeoSettingSavedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var handlerFolders = GetDefaultHandlerFolders();
                var item = new SeoSetting
                {
                    ContentTypeKey = notification.ContentTypeGuid,
                    IsEnabled = notification.IsEnabled
                };
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

        protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(SeoSetting parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return Task.FromResult(Enumerable.Empty<uSyncAction>());
        }

        protected override Task<IEnumerable<SeoSetting>> GetChildItemsAsync(SeoSetting? parent)
        {
            if (parent is not null)
                return Task.FromResult(Enumerable.Empty<SeoSetting>());
            return Task.FromResult(_seoSettingsService.GetAll().Select(it => new SeoSetting
            {
                ContentTypeKey = it.Key,
                IsEnabled = it.Value
            }));
        }

        protected override Task<IEnumerable<SeoSetting>> GetFoldersAsync(SeoSetting? parent)
        {
            return Task.FromResult(Enumerable.Empty<SeoSetting>());
        }

        protected override Task<SeoSetting?> GetFromServiceAsync(SeoSetting? item)
        {
            if (item is null)
                return Task.FromResult<SeoSetting?>(null);
            var contentType = _contentTypeService.Get(item.ContentTypeKey);
            if (contentType is null)
                return Task.FromResult<SeoSetting?>(null);

            return Task.FromResult<SeoSetting?>(new SeoSetting
            {
                ContentTypeKey = item.ContentTypeKey,
                IsEnabled = _seoSettingsService.IsEnabled(contentType)
            });
        }

        protected override string GetItemName(SeoSetting item)
        {
            var contentType = _contentTypeService.Get(item.ContentTypeKey);
            return contentType.Name ?? item.ContentTypeKey.ToString();
        }
    }
}
