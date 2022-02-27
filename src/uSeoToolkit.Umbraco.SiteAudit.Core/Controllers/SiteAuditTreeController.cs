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
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace uSeoToolkit.Umbraco.SiteAudit.Core.Controllers
{
    [Tree("uSeoToolkit", "SiteAudit", TreeTitle = "Site Audits", TreeGroup = "uSeoToolkit", SortOrder = 1)]
    [PluginController("uSeoToolkitSiteAudit")]
    public class SeoToolkitTreeController : TreeController
    {
        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public SeoToolkitTreeController(ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator,
            IMenuItemCollectionFactory menuItemCollectionFactory)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
            _menuItemCollectionFactory = menuItemCollectionFactory;
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-diagnostics";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/list";

            return root.Value;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            if (id == UmbConstants.System.RootString)
            {
                var menuItemCollection = _menuItemCollectionFactory.Create();
                var item = menuItemCollection.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true);
                item.NavigateToRoute($"{queryStrings.GetRequiredValue<string>("application")}/{TreeAlias}/create");

                return menuItemCollection;
            }

            return null;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return null;
        }
    }
}
