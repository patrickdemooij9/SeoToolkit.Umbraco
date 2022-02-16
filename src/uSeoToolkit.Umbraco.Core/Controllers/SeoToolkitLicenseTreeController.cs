using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace uSeoToolkit.Umbraco.Core.Controllers
{
    [Tree("uSeoToolkit", "License", TreeTitle = "uSeoToolkit", TreeGroup = "uSeoToolkit", SortOrder = 2)]
    [PluginController("uSeoToolkit")]
    public class SeoToolkitLicenseTreeController : TreeController
    {
        public SeoToolkitLicenseTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            return new TreeNodeCollection();
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var node = CreateTreeNode("license", "-1", queryStrings, "License", "icon-script", false,
                $"{SectionAlias}/{TreeAlias}/licenseDashboard");

            return node;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        {
            return null;
        }
    }
}
