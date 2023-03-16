using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Common.Core.Interfaces;
using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Models.Converters;
using SeoToolkit.Umbraco.MetaFields.Core.Models.DocumentTypeSettings.Business;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField;
using SeoToolkit.Umbraco.MetaFields.Core.Services.DocumentTypeSettings;
using Umbraco.Cms.Core.Web;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Core.Serializers;

[SyncSerializer("dce705fa-7259-4754-8d6b-1eeeb05f5f12", "SeoToolkit.MetaFieldSerializer",
    Constants.Serialization.MetaFieldValues)]
public class MetaFieldSettingsSerializer : SyncSerializerRoot<DocumentTypeSettingsDto>, ISyncSerializer<DocumentTypeSettingsDto>
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IRepository<DocumentTypeSettingsDto> _repository;
    private readonly IMetaFieldsSettingsService _metaFieldsSettingsService;

    public MetaFieldSettingsSerializer(IMetaFieldsSettingsService metaFieldsSettingsService,
        IUmbracoContextFactory umbracoContextFactory, ILogger<MetaFieldSettingsSerializer> logger,
        IRepository<DocumentTypeSettingsDto> repository) : base(logger)
    {
        _metaFieldsSettingsService = metaFieldsSettingsService;
        _umbracoContextFactory = umbracoContextFactory;
        _repository = repository;
    }

    private const string Value = "Value";
    private const string Fields = "Fields";
    private const string Editor = "Editor";
    private const string EditEditor = "EditEditor";
    private const string SeoField = "SeoField";

    protected override SyncAttempt<XElement> SerializeCore(DocumentTypeSettingsDto item, SyncSerializerOptions options)
    {
        var node = InitializeNode(item);

        var settingsDto = _repository.Get(item.Content.Id);

        if (settingsDto != null)
        {
            foreach (var el in from field in settingsDto.Fields let value = JsonSerializer.Serialize(field.Value) let editor = JsonSerializer.Serialize(field.Key.Editor) let editEditor = JsonSerializer.Serialize(field.Key.EditEditor) select new XElement(field.Key.Alias,
                         new XElement(SeoField, field.Key.Alias),
                         new XElement(Value, value),
                         new XElement(Editor, editor),
                         new XElement(EditEditor, editEditor)))
            {
                node.Add(el);
            }
        }
        else
        {
            return SyncAttempt<XElement>.Succeed(item.Content.Name, node, typeof(DocumentTypeSettingsDto),
                ChangeType.NoChange);
        }


        return SyncAttempt<XElement>.Succeed(item.Content.Name, node, typeof(DocumentTypeSettingsDto),
            ChangeType.Export);
    }


    private XElement InitializeNode(DocumentTypeSettingsDto item)
    {
        return new XElement(Constants.Serialization.RootName, new XAttribute("Key", ItemKey(item)));
    }

    protected override SyncAttempt<DocumentTypeSettingsDto> DeserializeCore(XElement node,
        SyncSerializerOptions options)
    {
        var attempt = FindItem(node);

        attempt.Fields = node.Elements().Select(element =>
        {
            var seoFieldAlias = element.Element(SeoField)?.Value;
            var valueJson = element.Element(Value)?.Value;
            if (seoFieldAlias == null || valueJson == null) return new KeyValuePair<ISeoField, DocumentTypeValueDto>();
            ISeoField field = seoFieldAlias switch
            {
                SeoFieldAliasConstants.Title => new SeoTitleField(),
                SeoFieldAliasConstants.CanonicalUrl => new CanonicalUrlField(),
                SeoFieldAliasConstants.MetaDescription => new SeoDescriptionField(),
                SeoFieldAliasConstants.OpenGraphDescription => new OpenGraphDescriptionField(),
                SeoFieldAliasConstants.OpenGraphImage => new OpenGraphImageField(_umbracoContextFactory),
                SeoFieldAliasConstants.OpenGraphTitle => new OpenGraphTitleField(),
                _ => new CanonicalUrlField()
            };

            var value = JsonSerializer.Deserialize<DocumentTypeValueDto>(valueJson);
            if (value?.Value != null && seoFieldAlias != SeoFieldAliasConstants.CanonicalUrl)
                value.Value = JsonSerializer.Deserialize<FieldsModel>(value.Value?.ToString()!);

            return new KeyValuePair<ISeoField, DocumentTypeValueDto>(field, value!);

        }).ToDictionary(x => x.Key, x => x.Value);


        return SyncAttempt<DocumentTypeSettingsDto>.Succeed(node.GetAlias(), attempt,
            typeof(DocumentTypeSettingsDto), ChangeType.Import);
    }

    public override bool IsValid(XElement node)
    {
        return true;
    }

    public override DocumentTypeSettingsDto FindItem(int id)
    {
        return _repository.Get(id);
    }

    public override DocumentTypeSettingsDto FindItem(Guid key)
    {
        return _repository.GetAll().FirstOrDefault(setting => setting.Content.Key == key)!;
    }

    public override DocumentTypeSettingsDto FindItem(string alias)
    {
        return _repository.GetAll().FirstOrDefault(setting => setting.Content.Alias == alias)!;
    }


    public override void SaveItem(DocumentTypeSettingsDto item)
    {
        _metaFieldsSettingsService.Set(item);
    }

    //TODO implement this
    public override void DeleteItem(DocumentTypeSettingsDto item)
    {
        _repository.Delete(item.Content.Id);
    }

    public override string ItemAlias(DocumentTypeSettingsDto item)
    {
        return item.Content.Alias;
    }

    public override Guid ItemKey(DocumentTypeSettingsDto item)
    {
        return item.Content.Key;
    }
}