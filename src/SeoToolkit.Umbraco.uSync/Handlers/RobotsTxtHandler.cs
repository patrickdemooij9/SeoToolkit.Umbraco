using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Strings;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.BackOffice.SyncHandlers.Interfaces;
using uSync.BackOffice.SyncHandlers.Models;
using uSync.Core;

namespace SeoToolkit.Umbraco.uSync.Handlers;

[SyncHandler("RobotsTxtHandler", "RobotsTxt Handler", "SeoToolkit", 3000, Icon = "icon-settings-alt")]
public class RobotsTxtHandler : SyncHandlerRoot<RobotsTxtModel, RobotsTxtModel>, ISyncHandler
{
    private readonly IRobotsTxtRepository _robotsTxtRepository;

    public RobotsTxtHandler(ILogger<SyncHandlerRoot<RobotsTxtModel, RobotsTxtModel>> logger,
     AppCaches appCaches,
      IShortStringHelper shortStringHelper,
       ISyncFileService syncFileService,
        ISyncEventService mutexService,
         ISyncConfigService uSyncConfig,
          ISyncItemFactory itemFactory,
          IRobotsTxtRepository robotsTxtRepository) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
    {
        _robotsTxtRepository = robotsTxtRepository;
    }

    protected override Task<IEnumerable<uSyncAction>> DeleteMissingItemsAsync(RobotsTxtModel parent, IEnumerable<Guid> keysToKeep, bool reportOnly) => Task.FromResult(Enumerable.Empty<uSyncAction>());

    protected override Task<IEnumerable<RobotsTxtModel>> GetChildItemsAsync(RobotsTxtModel? parent)
    {
        if (parent != null) return Task.FromResult(Enumerable.Empty<RobotsTxtModel>());

        return Task.FromResult(_robotsTxtRepository.GetAll());
    }

    protected override Task<IEnumerable<RobotsTxtModel>> GetFoldersAsync(RobotsTxtModel? parent) => Task.FromResult(Enumerable.Empty<RobotsTxtModel>());

    protected override Task<RobotsTxtModel?> GetFromServiceAsync(RobotsTxtModel? item) => Task.FromResult(item is null ? null : _robotsTxtRepository.Get(item.Id));

    protected override string GetItemName(RobotsTxtModel item) => item.Id.ToString();
}