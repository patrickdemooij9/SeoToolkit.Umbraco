namespace SeoToolkit.Umbraco.MetaFields.Core.Controllers
{
    /*[Tree("SeoToolkit", "MetaFields", TreeTitle = "MetaFields", TreeGroup = "SeoToolkit", SortOrder = 5)]
    [PluginController("SeoToolkit")]
    public class MetaFieldsTreeController : TreeController
    {
        public MetaFieldsTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.Icon = "icon-trafic";
            root.Value.HasChildren = false;
            root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/settings";
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
