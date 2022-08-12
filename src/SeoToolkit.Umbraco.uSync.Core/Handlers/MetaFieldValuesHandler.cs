using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.uSync.Core.Serializers;
using SeoToolkit.Umbraco.uSync.Core.XmlTrackers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.Core;
using uSync.Core.Serialization;

namespace uSync.BackOffice.SyncHandlers.Handlers;

[SyncHandler("seoToolkitMetaFieldValuesHandler", "SeoToolkit Meta Fields", "SeoToolkitMetaFields", uSyncConstants.Priorites.Content
    , Icon = "icon-list", IsTwoPass = true, EntityType = Constants.UdiEntityType.Document)]
public class MetaFieldValuesHandler : ContentHandlerBase<IContent, IContentService>, ISyncHandler
{
    private readonly IMetaFieldsValueService _metaFieldsValueService;
    private readonly MetaFieldValuesSerializer _metaFieldValuesSerializer;
    private readonly IContentService _contentService;

    /// <summary>
    ///  the default group for which events matter (content group)
    /// </summary>
    public override string Group => uSyncConstants.Groups.Content;

    public MetaFieldValuesHandler(MetaFieldValuesSerializer metaFieldValuesSerializer,IContentService contentService, ILogger<MetaFieldValuesHandler> logger,
        IEntityService entityService, AppCaches appCaches, IShortStringHelper shortStringHelper,
        SyncFileService syncFileService, uSyncEventService mutexService, uSyncConfigService uSyncConfigService,
        ISyncItemFactory syncItemFactory) : base(logger, entityService, appCaches, shortStringHelper, syncFileService,
        mutexService, uSyncConfigService, syncItemFactory)
    {

        _metaFieldValuesSerializer = metaFieldValuesSerializer;
        _contentService = contentService;

        this.serializer = _metaFieldValuesSerializer as ISyncSerializer<IContent>;
    }

    public override IEnumerable<uSyncAction> Export(IContent? item, string folder, HandlerSettings config)
    {
        if (item == null)
            return uSyncAction.Fail(nameof(item), typeof(IContent).ToString(), ChangeType.Fail, "Item not set",
                new ArgumentNullException(nameof(item))).AsEnumerableOfOne();

        var filename = GetPath(folder, item, config.GuidNames, config.UseFlatStructure)
            .ToAppSafeFileName();
        
        var attempt = _metaFieldValuesSerializer.Serialize(item, new SyncSerializerOptions(config.Settings));
        if (!attempt.Success)
            return uSyncActionHelper<XElement>.SetAction(attempt, filename, GetItemKey(item), this.Alias)
                .AsEnumerableOfOne();
        
        if (ShouldExport(attempt.Item, config))
        {
            // only write the file to disk if it should be exported.
            syncFileService.SaveXElement(attempt.Item, filename);
        }
        else
        {
            return uSyncAction.SetAction(true, filename, type: typeof(IContent).ToString(),
                    change: ChangeType.NoChange, message: "Not Exported (Based on config)", filename: filename)
                .AsEnumerableOfOne();
        }

        return uSyncActionHelper<XElement>.SetAction(attempt, filename, GetItemKey(item), this.Alias)
            .AsEnumerableOfOne();
    }


    public override IEnumerable<uSyncAction> Import(XElement node, string filename, HandlerSettings config, SerializerFlags flags)
    {
        if (ShouldImport(node, new HandlerSettings()))
        {
            var attempt = _metaFieldValuesSerializer.Deserialize(node, new SyncSerializerOptions(config.Settings));
            return uSyncActionHelper<IContent>.SetAction(attempt, filename,node.GetKey(), this.Alias)
                .AsEnumerableOfOne();
        }

        return uSyncAction.SetAction(true, filename, type: typeof(IContent).ToString(),
                change: ChangeType.NoChange, message: "Not Imported (Based on config)", filename: filename)
            .AsEnumerableOfOne();
    }


    public override IEnumerable<uSyncAction> ReportFolder(string folder, HandlerSettings config, SyncUpdateCallback callback)
    {
        var reportActions= base.ReportFolder(folder, config, callback).ToList();
        reportActions = reportActions.Select(action =>
        {
            if (!action.Details.Any())
            {
                return action;
            }

            var details = action.Details.ToList();
            //First item will always be an error since the default XmlTracker will have a xml schema mismatch error.
            details.RemoveAt(0);
            action.Details = details;
            return action;
        }).ToList();
       
        return reportActions;
    }
}