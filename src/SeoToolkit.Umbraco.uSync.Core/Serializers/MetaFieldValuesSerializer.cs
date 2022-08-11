using System.Drawing;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Core.Mapping;
using uSync.Core.Models;
using uSync.Core.Serialization;
using uSync.Core.Serialization.Serializers;

namespace SeoToolkit.Umbraco.uSync.Core.Serializers;

[SyncSerializer("cfeaa2d0-8af5-4ef4-9dd6-3b696df18189", "SeoToolkit.MetaFieldSerializer", Constants.Serialization.MetaFieldValues)]
public class MetaFieldValuesSerializer : ContentSerializerBase<IContent>
{
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IEntityService _entityService;
    private readonly IMetaFieldsValueService _metaFieldsValueService;
    private readonly IContentService _contentService;

    public MetaFieldValuesSerializer(IEntityService entityService, ILocalizationService localizationService,
        IRelationService relationService, IShortStringHelper shortStringHelper,
        ILogger<MetaFieldValuesSerializer> logger,
        SyncValueMapperCollection syncMappers, IMetaFieldsValueService metaFieldsValueService,
        IContentService contentService) : base(entityService, localizationService, relationService, shortStringHelper,
        logger, UmbracoObjectTypes.Document, syncMappers)
    {
        _entityService = entityService;
        _shortStringHelper = shortStringHelper;
        _metaFieldsValueService = metaFieldsValueService;
        _contentService = contentService;
    }

    private XElement InitializeNode(IContent item)
    {
        return new XElement(Constants.Serialization.RootName, new XAttribute("Key", ItemKey(item)), new XAttribute("Alias", ItemAlias(item)));
    }

    public new SyncAttempt<IContent> Deserialize(XElement node, SyncSerializerOptions options)
    {
        if (node.IsEmptyItem())
            return ProcessAction(node, options);
        if (!IsValid(node))
            throw new FormatException("XML Not valid for type " + this.ItemType);
        if (!options.Force && this.IsCurrent(node, options) <= ChangeType.NoChange)
            return SyncAttempt<IContent>.Succeed(node.GetAlias(), ChangeType.NoChange);
        var syncAttempt1 = this.CanDeserialize(node, options);
        if (!syncAttempt1.Success)
            return syncAttempt1;
        logger.LogDebug("Base: Deserializing {0}", (object) this.ItemType);
        var syncAttempt2 = DeserializeCore(node, options);
        if (syncAttempt2.Success)
        {
            logger.LogDebug("Base: Deserialize Core Success {0}", (object) this.ItemType);
            if (!syncAttempt2.Saved && !options.Flags.HasFlag((Enum) SerializerFlags.DoNotSave))
            {
                logger.LogDebug("Base: Serializer Saving (No DoNotSaveFlag) {0}", (object) this.ItemAlias(syncAttempt2.Item));
                SaveItem(syncAttempt2.Item);
            }
            if (options.OnePass)
            {
                logger.LogDebug("Base: Processing item in one pass {0}", (object) this.ItemAlias(syncAttempt2.Item));
                var syncAttempt3 = this.DeserializeSecondPass(syncAttempt2.Item, node, options);
                logger.LogDebug("Base: Second Pass Result {0} {1}", (object) this.ItemAlias(syncAttempt2.Item), (object) syncAttempt3.Success);
                return syncAttempt3;
            }
        }
        return syncAttempt2;
    }

    protected override SyncAttempt<XElement> SerializeCore(IContent item, SyncSerializerOptions options)
    {
        var node = InitializeNode(item);

        var fields = _metaFieldsValueService.GetUserValues(item.Id);

        if (!fields.Any())
        {
            return SyncAttempt<XElement>.Fail(item.Name, ChangeType.NoChange, "No Meta Fields");
        }

        node.Add(fields.Select(field => new XElement(field.Key, field.Value)));

        return SyncAttempt<XElement>.Succeed(item.Name, node, typeof(IContent), ChangeType.Export);
    }

    protected override SyncAttempt<IContent> DeserializeCore(XElement node, SyncSerializerOptions options)
    {
        var attempt = FindOrCreate(node);
        var item = GetDictionaryValues(node);
        if (attempt.Success && attempt.Result != null)
        {
            Save(attempt.Result.Id, item);
            return SyncAttempt<IContent>.Succeed(node.GetAlias(), attempt.Result,
                typeof(IContent), ChangeType.Import);
        }


        return SyncAttempt<IContent>.Fail(node.GetAlias(), attempt.Result, ChangeType.Fail,
            "Failed to find or create content", attempt.Exception);
    }

    public override bool IsValid(XElement node) => node != null! && node.GetAlias() != null && node.Name == Constants.Serialization.RootName;

    public override ChangeType IsCurrent(XElement node, SyncSerializerOptions options)
    {
        var content = _contentService.GetById(node.GetKey());
        var currentValues = _metaFieldsValueService.GetUserValues(content.Id);
        var newValues = GetDictionaryValues(node);
        
        if (currentValues.Count != newValues.Count)
            return ChangeType.Import;

        if (currentValues.Intersect(newValues).Count() != currentValues.Count)
            return ChangeType.Import;
        
        
        return ChangeType.NoChange;
    }
    
    public override IContent FindItem(int id)
    {
        var item = _contentService.GetById(id);
        if (item != null)
        {
            AddToNameCache(id, item.Key, item.Name);
            return item;
        }

        return null!;
    }
    

    public override IContent FindItem(Guid key)
        => _contentService.GetById(key)!;

    public override void SaveItem(IContent item)
    {
       //We do not need to save the contentItem itself, but we do need to override this method
    }

    public override void DeleteItem(IContent item)
    {
        //We do not need to delete the contentItem itself, but we do need to override this method
        //Maybe we should delete meta-field values for a content item when it is deleted?
    }


    private void Save(int nodeId, Dictionary<string, object?> item)
    {
        _metaFieldsValueService.AddValues(nodeId, item);
    }

    private Dictionary<string, object> GetDictionaryValues(XElement node)
    {
        return node.Descendants()
            .ToDictionary<XElement?, string, object>(descendantNode => descendantNode.Name.ToString(),
                descendantNode => descendantNode.Value);
    }


    protected override Attempt<IContent> CreateItem(string alias, ITreeEntity parent, string itemType)
    {
        var parentId = parent != null! ? parent.Id : -1;
        var item = _contentService.Create(alias, parentId, itemType);
        if (item == null!)
            return Attempt.Fail(item, new ArgumentException($"Unable to create content item of type {itemType}"))!;

        return Attempt.Succeed(item)!;
    }

    protected override uSyncChange HandleTrashedState(IContent item, bool trashed)
    {
        // We do not need to handle trashed state, the default handler does this for us.
        return null!;

    }

    protected override IContent FindAtRoot(string alias)
    {
        var rootNodes = _contentService.GetRootContent().ToList();
        if (rootNodes.Any())
        {
            return rootNodes.FirstOrDefault(x => x.Name!.ToSafeAlias(shortStringHelper).InvariantEquals(alias))!;
        }

        return null!;
    }
}