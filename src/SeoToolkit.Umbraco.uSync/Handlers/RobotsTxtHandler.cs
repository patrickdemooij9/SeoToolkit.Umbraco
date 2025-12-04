using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Notifications;
using System;
using System.Collections.Generic;
using System.Text;
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
    /*[SyncHandler("RobotsTxtHandler", "RobotsTxtHandler", "SeoToolkit", 3000, Icon = "icon-shield")]
    public class RobotsTxtHandler : SyncHandlerRoot<RobotsTxtModel, RobotsTxtModel>, ISyncHandler, INotificationAsyncHandler<RobotsTxtSavedNotification>
    {
        private readonly IRobotsTxtService _robotsTxtService;

        public RobotsTxtHandler(ILogger<SyncHandlerRoot<RobotsTxtModel, RobotsTxtModel>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, ISyncFileService syncFileService, ISyncEventService mutexService, ISyncConfigService uSyncConfig, ISyncItemFactory itemFactory, IRobotsTxtService robotsTxtService) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
        {
            _robotsTxtService = robotsTxtService;
        }

        public override string Group => "Settings";

        public async Task HandleAsync(RobotsTxtSavedNotification notification, CancellationToken cancellationToken)
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

        protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(RobotsTxtModel parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
        {
            return Task.FromResult(Enumerable.Empty<uSyncAction>());
        }

        protected override Task<IEnumerable<RobotsTxtModel>> GetChildItemsAsync(RobotsTxtModel? parent)
        {
            if (parent != null)
            {
                return Task.FromResult(Enumerable.Empty<RobotsTxtModel>());
            }

            return Task.FromResult<IEnumerable<RobotsTxtModel>>(_robotsTxtService.GetAll());
        }

        protected override Task<IEnumerable<RobotsTxtModel>> GetFoldersAsync(RobotsTxtModel? parent)
        {
            return Task.FromResult(Enumerable.Empty<RobotsTxtModel>());
        }

        protected override Task<RobotsTxtModel?> GetFromServiceAsync(RobotsTxtModel? item)
        {
            return Task.FromResult(item is null ? null : _robotsTxtService.Get(item.Key));
        }

        protected override string GetItemName(RobotsTxtModel item)
        {
            return "RobotsTxt";
        }
    }*/
}
