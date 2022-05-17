using SeoToolkit.Umbraco.MetaFields.Core.Constants;
using SeoToolkit.Umbraco.MetaFields.Core.Interfaces.SeoField;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.SeoFieldGroups
{
    public class OthersFieldGroup : ISeoFieldGroup
    {
        public string Alias => SeoFieldGroupConstants.Others;
        public string Name => "Other fields";
        public string Description => "Fields that don't get displayed, but are useful for indexing";
        public ISeoFieldPreviewer Previewer { get; }
    }
}
