using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace SeoToolkit.Umbraco.Redirects.Core.Controllers
{
    [Tree("SeoToolkit", "Redirects", TreeTitle = "Redirects", TreeGroup = "SeoToolkit", SortOrder = 4)]
    [PluginController("SeoToolkit")]
    public class RedirectsTreeController : TreeController
    {
        public RedirectsTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-trafic";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/list";
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
    }
}
