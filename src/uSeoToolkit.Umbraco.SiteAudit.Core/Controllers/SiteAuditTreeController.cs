using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using uSeoToolkit.Umbraco.Core.Constants;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    [Tree("uSeoToolkit", "SiteAudit", TreeTitle = "uSeoToolkit", TreeGroup = "uSeoToolkit", SortOrder = 1)]
    [PluginController("uSeoToolkitSiteAudit")]
    public class SeoToolkitTreeController : TreeController
    {
        public IMenuItemCollectionFactory MenuItemCollectionFactory { get; }

        public SeoToolkitTreeController(ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator,
            IMenuItemCollectionFactory menuItemCollectionFactory) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            MenuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return new TreeNodeCollection();
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var node = CreateTreeNode("siteAudit", "-1", queryStrings, "Site Audits", "icon-diagnostics", false,
                $"{SectionAlias}/{TreeAlias}/list");

            return node;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            var menuItemCollection = MenuItemCollectionFactory.Create();
            if (id == "siteAudit")
            {
                var item = menuItemCollection.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                item.NavigateToRoute($"{queryStrings.GetRequiredValue<string>("application")}/SiteAudit/create");
            }

            return menuItemCollection;
        }
    }
}
