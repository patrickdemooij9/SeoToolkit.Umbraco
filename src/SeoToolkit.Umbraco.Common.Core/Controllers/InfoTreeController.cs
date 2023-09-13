using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Constants;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace SeoToolkit.Umbraco.Common.Core.Controllers
{
    //This controller is only here to prevent single node trees if you only download one package
    [Tree("SeoToolkit", "info", TreeTitle = "Info", TreeGroup = TreeGroupAlias, SortOrder = 99)]
    public class InfoTreeController : TreeController
    {
        public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

        public InfoTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-info";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"SeoToolkit";
            root.Value.MenuUrl = null;

            return root.Value;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return TreeNodeCollection.Empty;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            return null;
        }
    }
}
