using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace SeoToolkit.Umbraco.Common.Core.Models.Business
{
    public class SeoToolkitTreeItemApiModel : TreeItemPresentationModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? ParentId { get; set; }
    }
}
