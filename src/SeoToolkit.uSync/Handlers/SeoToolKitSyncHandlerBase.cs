using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Strings;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.Core;


namespace SeoToolkit.uSync.Handlers;
public abstract class SeoToolKitSyncHandlerBase<TObject> : SyncHandlerRoot<TObject, TObject>
{
    protected override IEnumerable<TObject> GetFolders(TObject parent)
        => Enumerable.Empty<TObject>();
    protected override TObject GetFromService(TObject item)
        => item;

    protected override bool ShouldImport(XElement node, HandlerSettings config)
    {
        return base.ShouldImport(node, config);
    }

    protected override bool ShouldExport(XElement node, HandlerSettings config)
    {
        return base.ShouldExport(node, config);
    }

    protected SeoToolKitSyncHandlerBase(ILogger<SeoToolKitSyncHandlerBase<TObject>> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, SyncFileService syncFileService, uSyncEventService mutexService, uSyncConfigService uSyncConfig, ISyncItemFactory itemFactory) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
    {
    }
}