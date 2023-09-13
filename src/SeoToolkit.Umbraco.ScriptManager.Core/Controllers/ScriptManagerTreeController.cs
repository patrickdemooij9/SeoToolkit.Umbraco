using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUglify.JavaScript.Syntax;
using SeoToolkit.Umbraco.Common.Core.Constants;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Controllers
{
    [Tree("SeoToolkit", "ScriptManager", TreeTitle = "Script Manager", TreeGroup = TreeGroupAlias, SortOrder = 2)]
    [PluginController("SeoToolkit")]
    public class ScriptManagerTreeController : TreeController
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

        public ScriptManagerTreeController(
            ILocalizedTextService localizedTextService,
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

            root.Value.Icon = "icon-script";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/list";

            return root.Value;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            if (id == UmbConstants.System.RootString)
            {
                var menuItemCollection = _menuItemCollectionFactory.Create();

                var item = menuItemCollection.Items.Add<ActionNew>(LocalizedTextService, opensDialog: true, hasSeparator: false);
                item.NavigateToRoute($"{SectionAlias}/{TreeAlias}/edit");

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
