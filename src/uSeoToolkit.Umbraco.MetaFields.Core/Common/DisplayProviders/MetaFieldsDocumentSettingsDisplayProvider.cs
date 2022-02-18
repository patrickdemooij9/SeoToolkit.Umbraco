using Umbraco.Cms.Core.Composing;
using uSeoToolkit.Umbraco.Common.Core.Interfaces;
using uSeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace uSeoToolkit.Umbraco.MetaFields.Core.Common.DisplayProviders
{
    [Weight(100)]
    public class MetaFieldsDocumentSettingsDisplayProvider : IDisplayProvider
    {
        public SeoSettingsDisplayViewModel Get(int contentTypeId)
        {
            return new SeoSettingsDisplayViewModel
            {
                Alias = "metaFields",
                Name = "Meta Fields",
                View = "/App_Plugins/uSeoToolkit/MetaFields/Interface/ContentApps/DocumentSettings/documentSettings.html"
            };
        }
    }
}
