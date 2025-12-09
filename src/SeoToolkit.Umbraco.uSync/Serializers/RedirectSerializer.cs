using Microsoft.Extensions.Logging;
using SeoToolkit.Umbraco.Redirects.Core.Interfaces;
using SeoToolkit.Umbraco.Redirects.Core.Models.Business;
using SeoToolkit.Umbraco.RobotsTxt.Core.Models.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using uSync.Core;
using uSync.Core.Models;
using uSync.Core.Serialization;

namespace SeoToolkit.Umbraco.uSync.Serializers
{
    /*[SyncSerializer("2900f2aa-ad41-404a-b869-713e4e7d7483", "Redirect Serializer", "Redirect")]
    public class RedirectSerializer : SyncSerializerRoot<Redirect>, ISyncSerializer<Redirect>
    {
        private readonly IRedirectsService _redirectsService;
        private readonly ILanguageService _languageService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public RedirectSerializer(ILogger<SyncSerializerRoot<Redirect>> logger,
            IRedirectsService redirectsService,
            ILanguageService languageService,
            IUmbracoContextFactory umbracoContextFactory) : base(logger)
        {
            _redirectsService = redirectsService;
            _languageService = languageService;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override Task DeleteItemAsync(Redirect item)
        {
            _redirectsService.Delete([item.Key]);
            return Task.CompletedTask;
        }

        public override Task<Redirect?> FindItemAsync(Guid key)
        {
            return Task.FromResult(_redirectsService.Get(key));
        }

        public override Task<Redirect?> FindItemAsync(string alias)
        {
            return Task.FromResult<Redirect?>(null);
        }

        public override string ItemAlias(Redirect item)
        {
            return item.Key.ToString();
        }

        public override Guid ItemKey(Redirect item)
        {
            return item.Key;
        }

        public override bool IsValid(XElement node)
        {
            return node.Name.LocalName == ItemType
                && node.GetKey() != Guid.Empty;
        }

        public override Task SaveItemAsync(Redirect item)
        {
            _redirectsService.Save(item);
            return Task.CompletedTask;
        }

        protected override async Task<SyncAttempt<Redirect>> DeserializeCoreAsync(XElement node, SyncSerializerOptions options)
        {
            var item = await FindItemAsync(node);
            item ??= new Redirect
            {
                Key = node.Element("Key").ValueOrDefault(Guid.NewGuid())
            };

            var infoNode = node.Element("Info");
            if (infoNode is null)
            {
                return SyncAttempt<Redirect>.Fail(node.GetAlias(), ChangeType.Fail, "No Info node");
            }

            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            item.IsEnabled = infoNode.Element("IsEnabled").ValueOrDefault(false);
            item.IsRegex = infoNode.Element("IsRegex").ValueOrDefault(false);
            var domain = infoNode.Element("Domain").ValueOrDefault<string?>(null);
            if (!string.IsNullOrWhiteSpace(domain))
            {
                item.Domain = ctx.UmbracoContext.Domains.GetAll(false).First(it => it.Name.Equals(domain));
            }
            item.CustomDomain = infoNode.Element("CustomDomain").ValueOrDefault<string?>(null);
            item.OldUrl = infoNode.Element("OldUrl").ValueOrDefault<string>(string.Empty);
            item.NewUrl = infoNode.Element("NewUrl").ValueOrDefault<string>(string.Empty);
            var newNodeId = infoNode.Element("NewNode").ValueOrDefault<Guid?>(null);
            if (newNodeId.HasValue)
            {
                item.NewNode = ctx.UmbracoContext.Content.GetById(newNodeId.Value);
                if (item.NewNode is null)
                    return SyncAttempt<Redirect>.Fail(node.GetAlias(), ChangeType.Fail, $"Could not find node with key: {newNodeId.Value}");
            }
            var newNodeCulture = infoNode.Element("NewNodeCulture").ValueOrDefault<string?>(null);
            if (!string.IsNullOrWhiteSpace(newNodeCulture))
            {
                item.NewNodeCulture = await _languageService.GetAsync(newNodeCulture);
                if (item.NewNodeCulture is null)
                    return SyncAttempt<Redirect>.Fail(node.GetAlias(), ChangeType.Fail, $"Could not find culture with isoCode: {newNodeCulture}");
            }
            item.CreatedBy = infoNode.Element("CreatedBy").ValueOrDefault<Guid?>(null);
            item.RedirectCode = infoNode.Element("RedirectCode").ValueOrDefault(301);

            return SyncAttempt<Redirect>.Succeed(ItemAlias(item), item, ChangeType.Import, []);
        }

        protected override Task<SyncAttempt<XElement>> SerializeCoreAsync(Redirect item, SyncSerializerOptions options)
        {
            var node = new XElement(ItemType,
                new XAttribute("Key", item.Key)
            );

            var info = new XElement("Info",
                new XElement("IsEnabled", item.IsEnabled),
                new XElement("IsRegex", item.IsRegex),
                new XElement("Domain", item.Domain?.Name),
                new XElement("CustomDomain", item.CustomDomain),
                new XElement("OldUrl", item.OldUrl),
                new XElement("NewUrl", item.NewUrl),
                new XElement("NewNode", item.NewNode?.Key),
                new XElement("NewNodeCulture", item.NewNodeCulture?.IsoCode),
                new XElement("CreatedBy", item.CreatedBy),
                new XElement("RedirectCode", item.RedirectCode)
            );

            node.Add(info);

            return Task.FromResult(SyncAttempt<XElement>.Succeed(item.Key.ToString(), node, ChangeType.Export, []));
        }
    }*/
}
