using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.Services;
using SeoToolkit.Umbraco.MetaFields.Core.Models.SeoSettings.Database;
using SeoToolkit.Umbraco.MetaFields.Core.Repositories.SeoValueRepository;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Core.Serializers;

[SyncSerializer("cfeaa2d0-8af5-4ef4-9dd6-3b696df18189", "SeoToolkit.MetaFieldSerializer",
    Constants.Serialization.MetaFieldValues)]
public class MetaFieldValuesSerializer : SyncSerializerRoot<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>, ISyncSerializer<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>
{
    private readonly IMetaFieldsValueRepository _metaFieldsValueRepository;
    private readonly IMetaFieldsValueService _metaFieldsValueService;
    private readonly IContentService _contentService;
    private readonly ILocalizationService _localizationService;
    

    
    public MetaFieldValuesSerializer(IMetaFieldsValueService metaFieldsValueService,  IMetaFieldsValueRepository metaFieldsValueRepository,IContentService contentService ,ILocalizationService localizationService ,ILogger<MetaFieldValuesSerializer> logger) : base(logger)
    {
        _metaFieldsValueService = metaFieldsValueService;
        _metaFieldsValueRepository = metaFieldsValueRepository;
        _contentService = contentService;
        _localizationService = localizationService;
    }

    static Dictionary<string,object?> MetaFieldsValueEntitiesToDictionary(IEnumerable<MetaFieldsValueEntity> items, string culture)
    {
        return items.Where(it => it.Culture == culture).ToDictionary(it => it.Alias, it =>(object?)it.UserValue);
    }
    

    private XElement InitializeNode(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        return new XElement(Constants.Serialization.RootName, new XAttribute("Key", ItemKey(item)),
            new XAttribute("Alias", ItemAlias(item)));
    }
    
    private const string Value = "Value";
    private const string Culture = "Culture";
    protected override SyncAttempt<XElement> SerializeCore(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item, SyncSerializerOptions options)
    {
        var node = InitializeNode(item);

        var languageVariants = _localizationService.GetAllLanguages();

        if(item == null)
            return SyncAttempt<XElement>.Fail("NaN", ChangeType.NoChange, "No Meta Fields");
        
        foreach (var languageVariant in languageVariants)
        {
            var fields = MetaFieldsValueEntitiesToDictionary(item.Value.Value, languageVariant.IsoCode);

            if (!fields.Any())
            {
                return SyncAttempt<XElement>.Fail(item.Value.Key.ToString(), ChangeType.NoChange, "No Meta Fields");
            }

            foreach (var field in fields)
            {
                var currentNode = node.Element(field.Key);
                if (currentNode == null)
                {
                    var element = new XElement(field.Key, new XElement(Value, field.Value, new XAttribute(Culture, languageVariant.IsoCode)));
                    node.Add(element);
                }
                else
                {
                    currentNode.Add(new XElement(Value, field.Value, new XAttribute(Culture, languageVariant.IsoCode)));
                }
       
            }
        }


        return SyncAttempt<XElement>.Succeed(item.Value.Key.ToString(), node, typeof(IContent), ChangeType.Export);
    }

    protected override SyncAttempt<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?> CanDeserialize(XElement node, SyncSerializerOptions options)
    {
        return base.CanDeserialize(node, options);
    }

    protected override SyncAttempt<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?> DeserializeCore(XElement node, SyncSerializerOptions options)
    {
        var id = GetId(node);
        var list = new List<MetaFieldsValueEntity>();
        KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? kv = new KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>(id, list);
        var lookup = new Dictionary<string, Dictionary<string,string>>();

        var currentNode = node.FirstNode as XElement;
        while (currentNode != null)
        {
            if (lookup.TryGetValue(currentNode!.Name.LocalName, out var fieldsDictionary))
            {
                foreach (var nodeValue in currentNode.Nodes())
                {
                    var elementValue = nodeValue as XElement;
                    if(elementValue == null)
                        continue;
                    fieldsDictionary.Add(elementValue.FirstAttribute!.Value, elementValue.Value);
                }
            }
            else
            {
                var tempKvList = new Dictionary<string, string>();
                foreach (var nodeValue in currentNode.Nodes())
                {
                    var elementValue = nodeValue as XElement;
                    if(elementValue == null)
                        continue;
                    tempKvList.Add(elementValue.FirstAttribute!.Value, elementValue.Value);
                }
                lookup.Add(currentNode!.Name.LocalName, tempKvList);
            }
            currentNode = currentNode.NextNode as XElement;
        }

        var first = lookup.FirstOrDefault().Value.ToList();
        for (int i = 0; i < first.Count; i++)
        {
            var culture = first[i].Key;
            foreach (var keyValuePair in lookup)
            {
                var entity = new MetaFieldsValueEntity()
                {
                    Alias = keyValuePair.Key,
                    NodeId = id,
                    UserValue = keyValuePair.Value[culture],
                    Culture = culture
                };
                list.Add(entity);
            }
        }



        return SyncAttempt<KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?>.Succeed(node.GetAlias(), kv,
            typeof(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>?), ChangeType.Import);
        

    }

    private const string Alias = "Alias";

    public static int GetId(XElement node)
    {
        if (int.TryParse(node.Attribute(Alias)?.Value, out var id))
        {
            return id;
        }

        return -1;
    }

    public override KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? FindItem(int id)
    {
        var value = _metaFieldsValueRepository.GetAll().Where(it => it.Key == id).SelectMany(it => it);
        return new KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>(id, value);
    }

    public override KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? FindItem(Guid key)
    {
        var content = _contentService.GetById(key);
        if (content == null)
            return null!;
        return FindItem(content.Id);
    }

    public override KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? FindItem(string alias)
    {
        throw new NotImplementedException();
    }

    public override void SaveItem(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        if(item == null)
            return;
        var id = item.Value.Key;
        foreach (var group in item.Value.Value.GroupBy(it => it.Culture))
        {
            var culture = group.Key;
            var values = MetaFieldsValueEntitiesToDictionary(group.ToList(), culture);
            _metaFieldsValueService.AddValues(id, values, culture);
        }
        
    }

    public override void DeleteItem(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        if(item == null)
            return;
        var id = item.Value.Key;
        foreach (var group in item.Value.Value.GroupBy(it => it.Culture))
        {
            var culture = group.Key;
            var aliases = MetaFieldsValueEntitiesToDictionary(group.ToList(), culture).Select(metaField => metaField.Key );
            foreach (var alias in aliases)
            {
                _metaFieldsValueService.Delete(id, alias, culture);
            }
        }
    }
    public override bool IsValid(XElement node)
    {
        return true;
    }
    public override string ItemAlias(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        if(item == null)
            return "";
        return item.Value.Key.ToString();
    }

    public override Guid ItemKey(KeyValuePair<int, IEnumerable<MetaFieldsValueEntity>>? item)
    {
        if(item == null)
            return Guid.Empty;
        var content = _contentService.GetById(item.Value.Key);
        return content?.Key ?? Guid.Empty;
    }
}