using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Notifications;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
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
    [SyncHandler("MetaFieldsSettingsHandler", "Meta field settings", "SeoToolkit//MetaFieldsSettings", 3200, Icon = "icon-rectangle-ellipsis")]
    public class MetaFieldsHandler : SyncHandlerRoot<DocumentTypeSettingsDto, DocumentTypeSettingsDto>, ISyncHandler,
        INotificationAsyncHandler<MetaFieldSettingsSavedNotification>
    {
        private readonly IMetaFieldsSettingsService _metaFieldsSettingsService;

        public MetaFieldsHandler(ILogger<SyncHandlerRoot<DocumentTypeSettingsDto, DocumentTypeSettingsDto>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, ISyncFileService syncFileService, ISyncEventService mutexService, ISyncConfigService uSyncConfig, ISyncItemFactory itemFactory, IMetaFieldsSettingsService metaFieldsSettingsService) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _metaFieldsSettingsService = metaFieldsSettingsService;
        }

        public async Task HandleAsync(MetaFieldSettingsSavedNotification notification, CancellationToken cancellationToken)
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

        protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(DocumentTypeSettingsDto parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return Task.FromResult(Enumerable.Empty<uSyncAction>());
        }

        protected override Task<IEnumerable<DocumentTypeSettingsDto>> GetChildItemsAsync(DocumentTypeSettingsDto? parent)
        {
            if (parent != null)
            {
                return Task.FromResult(Enumerable.Empty<DocumentTypeSettingsDto>());
            }
            var allSettings = _metaFieldsSettingsService.GetAll();
            return Task.FromResult<IEnumerable<DocumentTypeSettingsDto>>(allSettings);
        }

        protected override Task<IEnumerable<DocumentTypeSettingsDto>> GetFoldersAsync(DocumentTypeSettingsDto? parent)
        {
            return Task.FromResult(Enumerable.Empty<DocumentTypeSettingsDto>());
        }

        protected override Task<DocumentTypeSettingsDto?> GetFromServiceAsync(DocumentTypeSettingsDto? item)
        {
            if (item is null)
                return Task.FromResult<DocumentTypeSettingsDto?>(null);
            return Task.FromResult(_metaFieldsSettingsService.Get(item.Content.Key));
        }

        protected override string GetItemName(DocumentTypeSettingsDto item)
        {
            return item.Content.Name ?? "Meta fields settings";
        }
    }
}
