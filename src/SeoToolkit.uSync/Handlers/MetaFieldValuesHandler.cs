using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Strings;
using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.Core;
using uSyncConstants = uSync.BackOffice.uSyncConstants;

namespace SeoToolkit.uSync.Handlers;

[SyncHandler("seoToolkitMetaFieldValuesHandler", "SeoToolkit Meta Fields", "SeoToolkitMetaFields", uSyncConstants.Priorites.Content
    , Icon = "icon-list", IsTwoPass = false, EntityType = Constants.UdiEntityType.Document)]
public class MetaFieldValuesHandler : SeoToolKitSyncHandlerBase<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>, ISyncHandler
{
    private readonly IMetaFieldsValueRepository _metaFieldsValueRepository;

    public MetaFieldValuesHandler(IMetaFieldsValueRepository metaFieldsValueRepository, ILogger<MetaFieldValuesHandler> logger, AppCaches appCaches, IShortStringHelper shortStringHelper, SyncFileService syncFileService, uSyncEventService mutexService, uSyncConfigService uSyncConfig, ISyncItemFactory itemFactory) : base(logger, appCaches, shortStringHelper, syncFileService, mutexService, uSyncConfig, itemFactory)
    {
        this.ItemType = "SeoToolkit Meta";
        _metaFieldsValueRepository = metaFieldsValueRepository;
    }

    //TODO this should ideally be implemented
    protected override IEnumerable<uSyncAction> DeleteMissingItems(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? parent, IEnumerable<Guid> keysToKeep, bool reportOnly)
    {
        return new List<uSyncAction>();
    }
    
    public IEnumerable<uSyncAction> ProcessPostImport(string folder, IEnumerable<uSyncAction> actions, HandlerSettings config)
    {
        return new List<uSyncAction>();
    }

    protected override IEnumerable<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?> GetChildItems(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? parent)
    {
        if (parent != null!) return new List<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>();
        var allField = _metaFieldsValueRepository.GetAll().Select(metaFieldGroup =>
        {
            KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? kv = new KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>(metaFieldGroup.Key, metaFieldGroup.ToList());
            return kv;
        });
        return allField;
    }

    protected override string GetItemName(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        if (item == null)
            return "";
        return item.Value.Key.ToString();
    }
    
}