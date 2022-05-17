using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;
using Umbraco.Extensions;

namespace SeoToolkit.Umbraco.MetaFields.Core.Models.SeoField.ViewModels
{
    public class SeoFieldGroupViewModel
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public SeoFieldGroupViewModel(ISeoFieldGroup group)
        {
            Alias = group.Alias;
            Name = group.Name;
            Description = group.Description;
        }
    }
}
