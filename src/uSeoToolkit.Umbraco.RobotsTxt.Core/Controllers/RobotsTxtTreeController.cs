using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace uSeoToolkit.Umbraco.RobotsTxt.Core.Controllers
{
    [Tree("uSeoToolkit", "RobotsTxt", TreeTitle = "uSeoToolkit", TreeGroup = "uSeoToolkit", SortOrder = 3)]
    [PluginController("uSeoToolkit")]
    public class RobotsTxtTreeController : TreeController
    {
        public RobotsTxtTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator,
            IMenuItemCollectionFactory menuItemCollectionFactory) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return new TreeNodeCollection();
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var node = CreateTreeNode("robotsTxt", "-1", queryStrings, "Robots.txt", "icon-cloud", false,
                $"{SectionAlias}/{TreeAlias}/detail");

            return node;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            return null;
        }
    }
}
