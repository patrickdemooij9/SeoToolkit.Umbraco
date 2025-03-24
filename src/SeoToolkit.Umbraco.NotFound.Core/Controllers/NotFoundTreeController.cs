using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeoToolkit.Umbraco.Common.Core.Constants;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;

namespace SeoToolkit.Umbraco.NotFound.Core.Controllers;

[Tree("SeoToolkit", Alias, TreeTitle = Title, TreeGroup = TreeGroupAlias, SortOrder = 6)]
[PluginController("SeoToolkit")]
public class NotFoundTreeController : TreeController
{
    public const string Alias = TreeControllerConstants.NotFound.Alias;
    public const string Title = TreeControllerConstants.NotFound.Title;
    public const string TreeGroupAlias = TreeControllerConstants.SeoToolkitTreeGroupAlias;

    public NotFoundTreeController(ILocalizedTextService localizedTextService, UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection, IEventAggregator eventAggregator) : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
    }

    protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
    {
        var root = base.CreateRootNode(queryStrings);

        if(root.Value == null)
        {
            return root;
        }

        root.Value.Icon = "icon-article";
        root.Value.HasChildren = false;
        root.Value.RoutePath = $"{SectionAlias}/{TreeAlias}/detail";
        root.Value.MenuUrl = null;

        return root.Value;
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        return null;

    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
    {
        return new ActionResult<MenuItemCollection>(new EmptyResult());
    }
}
