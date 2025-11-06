using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Interfaces;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers;

[SyncSerializer("c27ab9ab-e0e0-4095-aef7-1a8f4a4d617b", "RobotsTxt Serializer", "RobotsTxtModel")]
public class RobotsTxtSerializer : SyncSerializerRoot<RobotsTxtModel>, ISyncSerializer<RobotsTxtModel>
{
    private readonly IRobotsTxtRepository _robotsTxtRepository;

    public RobotsTxtSerializer(ILogger<SyncSerializerRoot<RobotsTxtModel>> logger, IRobotsTxtRepository robotsTxtRepository) : base(logger)
    {
        _robotsTxtRepository = robotsTxtRepository;
    }

    public override Task DeleteItemAsync(RobotsTxtModel item)
    {
        item.Content = string.Empty;
        _robotsTxtRepository.Update(item);
        return Task.CompletedTask;
    }

    public override Task<RobotsTxtModel?> FindItemAsync(Guid key)
    {
        return Task.FromResult(default(RobotsTxtModel));
    }

    public override Task<RobotsTxtModel?> FindItemAsync(string alias)
    {
        if (int.TryParse(alias, out var id))
        {
            var model = _robotsTxtRepository.Get(id);
            return Task.FromResult<RobotsTxtModel?>(model);
        }

        return Task.FromResult<RobotsTxtModel?>(new RobotsTxtModel());
    }

    public override string ItemAlias(RobotsTxtModel item)
    {
        return item.Id.ToString();
    }

    public override Guid ItemKey(RobotsTxtModel item)
    {
        return Guid.Empty;
    }

    public override Task SaveItemAsync(RobotsTxtModel item)
    {
        _robotsTxtRepository.Update(item);
        return Task.CompletedTask;
    }

    protected override async Task<SyncAttempt<RobotsTxtModel>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
    {
        var item = await FindItemAsync(node);
        Console.WriteLine(item);
        if (item is null)
        {
            return SyncAttempt<RobotsTxtModel>.Fail($"RobotsTxt with alias '{ItemAlias(item!)}' not found", ChangeType.Fail, "Cannot find RobotsTxt");
        }

        var infoNode = node.Element("Info");
        Console.WriteLine(infoNode);
        if (infoNode is null)
        {
            return SyncAttempt<RobotsTxtModel>.Fail("No info node found", ChangeType.Fail, "Invalid data");
        }

        item.Content = infoNode.Element("Content").ValueOrDefault(string.Empty);
        item.DomainId = infoNode.Element("DomainId").ValueOrDefault((int?)null);

        return SyncAttempt<RobotsTxtModel>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
    }

    protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(RobotsTxtModel item, SyncSerializerOptions options)
    {
        var alias = ItemAlias(item);

        var node = new XElement(ItemType, new XAttribute("Alias", alias));

        var info = new XElement("Info",
            new XElement("Content", item.Content),
            new XElement("DomainId", item.DomainId.HasValue ? item.DomainId.Value : null)
        );

        node.Add(info);

        return Task.FromResult(SyncAttempt<XElement>.Succeed(alias, node, ChangeType.Export, []));
    }
}