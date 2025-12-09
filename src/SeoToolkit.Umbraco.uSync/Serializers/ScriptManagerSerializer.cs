using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using SeoToolkit.Umbraco.ScriptManager.Core.Interfaces.Services;
using SeoToolkit.Umbraco.ScriptManager.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    /*public class ScriptManagerSerializer : SyncSerializerRoot<Script>, ISyncSerializer<Script>
    {
        private readonly IScriptManagerService _scriptManagerService;

        public ScriptManagerSerializer(ILogger<SyncSerializerRoot<Script>> logger, IScriptManagerService scriptManagerService) : base(logger)
        {
            _scriptManagerService = scriptManagerService;
        }

        public override Task DeleteItemAsync(Script item)
        {
            _scriptManagerService.Delete([item.Key!.Value]);
            return Task.CompletedTask;
        }

        public override Task<Script?> FindItemAsync(Guid key)
        {
            return Task.FromResult(_scriptManagerService.Get(key));
        }

        public override Task<Script?> FindItemAsync(string alias)
        {
            return Task.FromResult<Script?>(null);
        }

        public override string ItemAlias(Script item)
        {
            return item.Key.ToString()!;
        }

        public override bool IsValid(XElement node)
        {
            return node.Name.LocalName == ItemType
                && node.GetKey() != Guid.Empty;
        }

        public override Guid ItemKey(Script item)
        {
            return item.Key!.Value;
        }

        public override Task SaveItemAsync(Script item)
        {
            _scriptManagerService.Save(item);
            return Task.CompletedTask;
        }

        protected override async Task<SyncAttempt<Script>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            item ??= new Script
            {
                Key = node.Element("Key").ValueOrDefault(Guid.NewGuid())
            };

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<Script>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            item.Name = infoNode.Element("Name").ValueOrDefault<string>(string.Empty);
            //item.Definition
            item.DomainId = infoNode.Element("DomainId").ValueOrDefault<Guid?>(null);

            return SyncAttempt<Script>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(Script item, SyncSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }*/
}
