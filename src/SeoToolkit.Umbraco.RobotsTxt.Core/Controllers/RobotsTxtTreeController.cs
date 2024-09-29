using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Constants;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;

namespace SeoToolkit.Umbraco.RobotsTxt.Core.Controllers
{
    /*[Tree("SeoToolkit", Alias, TreeTitle = Title, TreeGroup = TreeGroupAlias, SortOrder = 3)]
    [PluginController("SeoToolkit")]
    public class RobotsTxtTreeController : TreeController
    {
        public const string Alias = TreeControllerConstants.RobotsTxt.Alias;
        public const string Title = TreeControllerConstants.RobotsTxt.Title;
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        public RobotsTxtTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        { }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-cloud";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/detail";
            root.Value.MenuUrl = null;

            return root.Value;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            return null;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return null;
        }
    }*/
}
