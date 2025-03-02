namespace SeoToolkit.Umbraco.Core.Controllers
{
    /*[Tree("SeoToolkit", "License", TreeTitle = "License", TreeGroup = "SeoToolkit", SortOrder = 20)]
    [PluginController("SeoToolkit")]
    public class SeoToolkitLicenseTreeController : TreeController
    {
        public SeoToolkitLicenseTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        { }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-pushpin";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/licenseDashboard";
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
